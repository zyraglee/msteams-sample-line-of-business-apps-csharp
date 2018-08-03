using Microsoft.Bot.Connector.Teams.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoAirline.Model
{

    //
    // Summary:
    //     O365 connector card section
    public class O365ConnectorCardSectionNew
    {
        //
        // Summary:
        //     Initializes a new instance of the O365ConnectorCardSection class.
        public O365ConnectorCardSectionNew() { }
        //
        // Summary:
        //     Initializes a new instance of the O365ConnectorCardSection class.
        //
        // Parameters:
        //   title:
        //     Title of the section
        //
        //   text:
        //     Text for the section
        //
        //   activityTitle:
        //     Activity title
        //
        //   activitySubtitle:
        //     Activity subtitle
        //
        //   activityText:
        //     Activity text
        //
        //   activityImage:
        //     Activity image
        //
        //   activityImageType:
        //     Describes how Activity image is rendered. Possible values include: 'avatar',
        //     'article'
        //
        //   markdown:
        //     Use markdown for all text contents. Default vaule is true.
        //
        //   facts:
        //     Set of facts for the current section
        //
        //   images:
        //     Set of images for the current section
        //
        //   potentialAction:
        //     Set of actions for the current section
        public O365ConnectorCardSectionNew(string title = null, string text = null, string activityTitle = null, string activitySubtitle = null, string activityText = null, string activityImage = null, string activityImageType = null, bool? markdown = null, IList<O365ConnectorCardFact> facts = null, IList<O365ConnectorCardImage> images = null, IList<O365ConnectorCardActionBase> potentialAction = null)
        {
}
        //
        // Summary:
        //     Gets or sets title of the section
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        //
        // Summary:
        //     Gets or sets text for the section
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        //
        // Summary:
        //     Gets or sets activity title
        [JsonProperty(PropertyName = "activityTitle")]
        public string ActivityTitle { get; set; }
        //
        // Summary:
        //     Gets or sets activity subtitle
        [JsonProperty(PropertyName = "activitySubtitle")]
        public string ActivitySubtitle { get; set; }
        //
        // Summary:
        //     Gets or sets activity text
        [JsonProperty(PropertyName = "activityText")]
        public string ActivityText { get; set; }
        //
        // Summary:
        //     Gets or sets activity image
        [JsonProperty(PropertyName = "activityImage")]
        public string ActivityImage { get; set; }
        //
        // Summary:
        //     Gets or sets describes how Activity image is rendered. Possible values include:
        //     'avatar', 'article'
        [JsonProperty(PropertyName = "activityImageType")]
        public string ActivityImageType { get; set; }
        //
        // Summary:
        //     Gets or sets use markdown for all text contents. Default vaule is true.
        [JsonProperty(PropertyName = "markdown")]
        public bool? Markdown { get; set; }
        //
        // Summary:
        //     Gets or sets set of facts for the current section
        [JsonProperty(PropertyName = "facts")]
        public IList<O365ConnectorCardFact> Facts { get; set; }
        //
        // Summary:
        //     Gets or sets set of images for the current section
        [JsonProperty(PropertyName = "images")]
        public IList<O365ConnectorCardImage> Images { get; set; }
        //
        // Summary:
        //     Gets or sets set of actions for the current section
        [JsonProperty(PropertyName = "potentialAction")]
        public IList<O365ConnectorCardActionBase> PotentialAction { get; set; }

        [JsonProperty(PropertyName = "potentialAction")]
        public Heroimage HeroImage { get; set; }
    }

    public class Heroimage
    {
        public string image { get; set; }
        public string title { get; set; }
    }

}