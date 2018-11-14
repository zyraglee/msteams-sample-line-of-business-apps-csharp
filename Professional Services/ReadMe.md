## Employee Leave App

This application has a bot and a couple of tabs that enable employees and managers to complete the entire work flow that they complete on a vacation tool. 

Employee Workflow 

Scenario 1: Set manager 

Enter the bot command as set manager and share the email Id. This helps show the manager workflow of receiving notifications and approval 

![1](Images/1.png)

Scenario 2: Apply for a leave

Ping hi to the bot and it will show up the adaptive card with the below inputs and choose leave request.

![2](Images/2.png)


Choosing a type of leave, refreshed the card to gather additional information and also pre-populates the kind of leave in the drop down. Ex: when you choose sickness, it shows in the drop down option only sick leave while for vacation / personal gives you options around Personal leave, optional leave etc. 

![3](Images/3.png)

Upon submitting the request, a confirmation message comes with options to edit & withdraw & also a notification goes to the manager 

![4](Images/4.png)

When the manager approves, the same card is refreshed to reflect the status 

![5](Images/5.png)

If you choose to withdraw, the card is refreshed for you and the manager that the request has been withdrawn 

![6](Images/6.png)

If you want to edit the card, it pulls up a task module to make changes 

![7](Images/7.png)

Scenario 3: Personal leave board 

This gives a quick overview of everything related to one's history of leave requests, status, and tips etc. 

![8](Images/8.png)​

Manager Workflow

Scenario 1: The main bot card in addition to the options to check for pending approvals too. This will also reflect in the Leave board tab  
 ![9](Images/9.png)
 

Scenario 2: Approving / rejecting a leave request 
 
When a employee makes a new request, a notification comes to the manager and a rich card is presented for approval / reject. The approval / reject action is updated for the employee too as shown in the flow above. 
 
![10](Images/10.png)
 
When the employee withdraws the leave request, the same card is updated to reflect the same too

![11](Images/11.png)
 
 
Scenario 3: Personal Leave board
This gives a quick overview of everything related to one's history of leave requests, status, and tips etc. Go to the personal scope of the app from the left rail and pin a tab to see this view. For manager's pending requests are also shown  
 
![12](Images/12.png)





