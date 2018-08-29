using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using System.Collections.Generic;
using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using Microsoft.Teams.Samples.HelloWorld.Web.Helper;
using System.Linq;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            using (var connector = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {
                if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
                {
                    //CreateDataRecords();
                    //if (activity.Value != null)
                    //{
                    //    InventoryInputDetails itemCount = Newtonsoft.Json.JsonConvert.DeserializeObject<InventoryInputDetails>(activity.Value.ToString());
                    //    Activity replyActivity = activity.CreateReply();
                    //    switch (itemCount.Type)
                    //    {
                    //        case Constants.newInventoryCount:
                    //            await AddItems(itemCount, replyActivity);
                               
                    //            await connector.Conversations.ReplyToActivityAsync(replyActivity);
                    //            break;
                    //        case Constants.BlockInventory:
                    //            await BlockItems(itemCount, replyActivity);
                    //            await connector.Conversations.ReplyToActivityAsync(replyActivity);
                    //            break;
                    //        case Constants.RetireInventory:
                    //            await RetireItems(itemCount, replyActivity);
                    //            await connector.Conversations.ReplyToActivityAsync(replyActivity);
                    //            break;
                    //        case Constants.RequestNewStock:
                    //            await AttachNewStock(replyActivity);
                               
                    //            await connector.Conversations.ReplyToActivityAsync(replyActivity);
                    //            break;
                               
                    //        default:
                    //            break; 
                    //    }

                    //}
                    //else
                    //{
                        await Conversation.SendAsync(activity, () => new EchoBot());
                    //}
                }
                else if (activity.Type == ActivityTypes.Invoke)
                {
                    if (activity.IsComposeExtensionQuery())
                    {
                        var response = MessageExtension.HandleMessageExtensionQuery(connector, activity);
                        return response != null
                            ? Request.CreateResponse<ComposeExtensionResponse>(response)
                            : new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    
                    else if (activity.IsO365ConnectorCardActionQuery())
                    {
                        
                        return await HandleO365ConnectorCardActionQuery(activity);
                    }
                }
                else if (activity.Type == ActivityTypes.Message)
                {
                    ConnectorClient connector1 = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply($"You sent {activity.Text} which was {activity.Text.Length} characters");

                    var msgToUpdate = await connector.Conversations.ReplyToActivityAsync(reply);
                    Activity updatedReply = activity.CreateReply($"This is an updated message");
                    await connector.Conversations.UpdateActivityAsync(reply.Conversation.Id, msgToUpdate.Id, updatedReply);
                }
              

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }

        }

        private static async Task<HttpResponseMessage> HandleO365ConnectorCardActionQuery(Activity activity)
        {
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));
            O365ConnectorCardActionQuery o365CardQuery = activity.GetO365ConnectorCardActionQueryData();
            Activity replyActivity = activity.CreateReply();
            try
            {
                if (o365CardQuery.ActionId == Constants.Industry)
                {
                    var industryCode = Newtonsoft.Json.JsonConvert.DeserializeObject<O365BodyValue>(o365CardQuery.Body);
                    await ShowProductInfo(industryCode.Value, replyActivity);

                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await connector.Conversations.ReplyToActivityAsync(replyActivity);
                }
                //else
                //    await Conversation.SendAsync(activity, () => new EchoBot());
            }
            catch(Exception e)
            {
                activity.CreateReply(e.Message.ToString());
            }
            //switch (o365CardQuery.ActionId)
            //{
            //    case Constants.Industry:
            //        var industryCode = Newtonsoft.Json.JsonConvert.DeserializeObject<O365BodyValue>(o365CardQuery.Body);
            //        await ShowProductInfo(industryCode.Value, replyActivity);
            //        break;
            //    case Constants.newInventoryCount:
            //        InventoryInputDetails itemCount = Newtonsoft.Json.JsonConvert.DeserializeObject<InventoryInputDetails>(o365CardQuery.Body);
            //        await AddItems(itemCount, replyActivity);
            //        break;
            //    case Constants.BlockInventory:
            //        InventoryInputDetails blockItem = Newtonsoft.Json.JsonConvert.DeserializeObject<InventoryInputDetails>(o365CardQuery.Body);                    
            //        await BlockItems(blockItem,replyActivity);
            //        break;
            //    case Constants.RetireInventory:
            //        InventoryInputDetails retireitemcount = Newtonsoft.Json.JsonConvert.DeserializeObject<InventoryInputDetails>(o365CardQuery.Body);
            //        await RetireItems(retireitemcount, replyActivity);
            //        break;
            //    case Constants.RequestNewStock:
            //        await AttachNewStock(replyActivity);
            //        break;
            //    default:
            //        break;
            //}
            //await connectorClient.Conversations.ReplyToActivityWithRetriesAsync(replyActivity);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        private static async Task ShowProductInfo(string IndustryCode, Activity replyActivity)
        {
            
            var list = await DocumentDBRepository<Product>.GetItemsAsync(d => d.IndustryCode.ToLower()==IndustryCode.ToLower() && d.IsActive==true);/* && d.JourneyDate.ToShortDateString()==flighinput.JourneyDate.ToShortDateString());*/

            
            if (list.Count() > 0)
            {
                var ListCard = AddListCardAttachment(replyActivity, list);
            }
            else
            {
                replyActivity.Text = $"Products not avilibile for selected Industry.";
            }
        }
        private static async Task AttachNewStock(Activity replyActivity)
        {
            replyActivity.Text = "Thanks for your request. The procurement team has been notified of your request" ;
        }

        private static async Task AddItems(InventoryInputDetails itemcount, Activity replyActivity)
        {
            var addItems = await DocumentDBRepository<Product>.GetItemsAsync(d => d.PrdouctId == Convert.ToInt32(itemcount.ProductId));
            if (addItems.Count() > 0)
            {
                try
                {
                    var list = addItems.FirstOrDefault();
                    foreach(var loc in list.locationList)
                    {
                        if(itemcount.Location.ToLower()==loc.Location.ToLower())
                        {
                            loc.Quantity = Convert.ToInt32(loc.Quantity) + Convert.ToInt32(itemcount.newItemCount);
                        }
                    }
                    
                    var itemsList = await DocumentDBRepository<Product>.UpdateItemAsync(list.Id, list);
                    var replyCard = O365CardHelper.GetAdativeCard(addItems.FirstOrDefault(), itemcount.ActionId);
                    replyActivity.Attachments.Add(replyCard);
                    //replyActivity.Text = $"Items added successfully {aircardInfo.AircraftId} has been assigned to Flight: {aircardInfo.FlightNumber}";
                    //replyActivity.Text = "Items added Successfully";
                }
                catch (Exception e)
                {
                    replyActivity.Text = e.Message.ToString();
                }
            }

        }

        private static async Task RetireItems(InventoryInputDetails itemcount, Activity replyActivity)
        {
            var addItems = await DocumentDBRepository<Product>.GetItemsAsync(d => d.PrdouctId == Convert.ToInt32(itemcount.ProductId));
            if (addItems.Count() > 0)
            {
                try
                {
                    var list = addItems.FirstOrDefault();
                    foreach (var loc in list.locationList)
                    {
                        if (itemcount.Location.ToLower() == loc.Location.ToLower())
                        {
                            loc.Quantity = Convert.ToInt32(loc.Quantity) - Convert.ToInt32(itemcount.newItemCount);
                        }
                    }
                    //list.Quantity = Convert.ToInt32(list.Quantity) - Convert.ToInt32(itemcount.newItemCount);
                    var itemsList = await DocumentDBRepository<Product>.UpdateItemAsync(list.Id, list);
                    var replyCard = O365CardHelper.GetAdativeCard(addItems.FirstOrDefault(), itemcount.ActionId);
                    replyActivity.Attachments.Add(replyCard);
                    //replyActivity.Text = $"Items added successfully {aircardInfo.AircraftId} has been assigned to Flight: {aircardInfo.FlightNumber}";
                    //replyActivity.Text = "Items retire Successfully";
                }
                catch (Exception e)
                {
                    replyActivity.Text = e.Message.ToString();
                }
            }

        }

        private static async Task BlockItems(InventoryInputDetails itemcount, Activity replyActivity)
        {
            var addItems = await DocumentDBRepository<Product>.GetItemsAsync(d => d.PrdouctId == Convert.ToInt32(itemcount.ProductId));
            if (addItems.Count() > 0)
            {
                try
                {
                    var list = addItems.FirstOrDefault();
                    foreach (var loc in list.locationList)
                    {
                        if (itemcount.Location.ToLower() == loc.Location.ToLower())
                        {
                            loc.Committed = Convert.ToInt32(loc.Committed) + Convert.ToInt32(itemcount.newItemCount);
                            if(loc.Quantity-loc.Committed>0)
                            {
                                var itemsList = await DocumentDBRepository<Product>.UpdateItemAsync(list.Id, list);
                                var replyCard = O365CardHelper.GetAdativeCard(addItems.FirstOrDefault(), itemcount.ActionId);
                                replyActivity.Attachments.Add(replyCard);
                            }
                            else
                            {
                                replyActivity.Text = "Items are not availbile";
                            }
                        }
                    }
                    //list.Quantity = Convert.ToInt32(list.Quantity) - Convert.ToInt32(itemcount);

                   
                    

                   
                }
                catch (Exception e)
                {
                    replyActivity.Text = e.Message.ToString();
                }
            }

        }

       
        
        private static async Task AddListCardAttachment(Activity replyActivity, System.Collections.Generic.IEnumerable<Product> productInfo)
        {
            var card = O365CardHelper.GetListofProducts(productInfo);
            try
            {
                replyActivity.Attachments.Add(card);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
        }

        private static async Task AttachRebookPassenger(string flightNumber, Activity replyActivity)
        {
            replyActivity.Text = $"Passenger has been rebooked on flight number: " + flightNumber;
        }

        private static async Task CreateDataRecords()
        {
            List<Product> lst = new List<Product>();
            Product obj5 = new Product();
            obj5.PrdouctId = 10;
            obj5.ProductName = "Casters";
            obj5.IsActive = true;
            //obj5.Quantity = 10;
            obj5.IndustryCode = "mft";
            //obj5.Location = "Hyderbad";

            List<Locationbased> lst1 = new List<Locationbased>();
            Locationbased obj = new Locationbased();
            obj.Location = "Hyderbad";
            obj.Quantity = 10;
            lst1.Add(obj);
            Locationbased obj12 = new Locationbased();
            obj12.Location = "Banaglore";
            obj12.Quantity = 10;
            lst1.Add(obj12);
            obj5.locationList = lst1;
            await DocumentDBRepository<Product>.CreateItemAsync(obj5);

            //Product obj6 = new Product();
            //obj6.PrdouctId = 10;
            //obj6.ProductName = "Frames";
            //obj6.IsActive = true;
            //obj6.Quantity = 10;
            //obj6.IndustryCode = "mft";
            //obj6.Location = "Hyderbad";
            //await DocumentDBRepository<Product>.CreateItemAsync(obj6);
            //Product obj7 = new Product();
            //obj7.PrdouctId = 11;
            //obj7.ProductName = "Ball bearings";
            //obj7.IsActive = true;
            //obj7.Quantity = 10;
            //obj7.IndustryCode = "mft";
            //obj7.Location = "Hyderbad";
            //await DocumentDBRepository<Product>.CreateItemAsync(obj7);
            //Product obj8 = new Product();
            //obj8.PrdouctId = 12;
            //obj8.ProductName = "Key stock";
            //obj8.IsActive = true;
            //obj8.Quantity = 10;
            //obj8.IndustryCode = "mft";
            //obj8.Location = "Hyderbad";
            //await DocumentDBRepository<Product>.CreateItemAsync(obj8);
            //Cities obj2 = new Cities();
            //obj2.CityCode = "EWR";
            //obj2.CityName = "Newark";
            //await DocumentDBRepository<Cities>.CreateItemAsync(obj2);
            //Cities obj3 = new Cities();
            //obj3.CityCode = "BWI";
            //obj3.CityName = "Washington, DC";
            //await DocumentDBRepository<Cities>.CreateItemAsync(obj3);
            //Cities obj4 = new Cities();
            //obj4.CityCode = "SEA";
            //obj4.CityName = "Boston";
            //await DocumentDBRepository<Cities>.CreateItemAsync(obj4);
            //Cities obj5 = new Cities();
            //obj5.CityCode = "JFK";
            //obj5.CityName = "New York";
            //await DocumentDBRepository<Cities>.CreateItemAsync(obj5);
            //Cities obj6 = new Cities();
            //obj6.CityCode = "ORD";
            //obj6.CityName = "Chicago";
            //await DocumentDBRepository<Cities>.CreateItemAsync(obj6);

        }



    }
}
