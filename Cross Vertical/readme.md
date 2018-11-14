## Notification:

This application lets the user to choose a trigger to initiate notification. There are three triggers here - weather, operations delay or social events. If operating in a team scope, upon choosing the trigger, select the people to whom the notification should be sent to. 

This will result in a notification being sent in a personal scope to the chosen members and also the card is updated in a channel scope. In addition to this it also increases the pill count in the activity feed indicating a new notification

![1](Images/1.png)

![2](Images/2.png)

## Employee Survey / Poll:

Scenario 1: setting an admin to get access to publish a survey 

Enter the command "set admin" and in the following dialogue box, enter your email ID to get access to create & publish a survey 

![3](Images/3.png)​​​

Scenario 2: Creating a survey 

The questions that this app supports are only multiple choice questions. The questions and the people to whom the survey should be published are shared with the bot in the form of a excel. The model excel sheet is attached in the Files section for use. 

Once you set admin, when you ping "hi" the bot gives you options to create a survey, download the results of a survey or even send reminder to those who have no filled the survey yet.

![4](Images/4.png)


Choose 'Create Survey' and share the excel sheet with details of questions, options and also members to whom this survey should be pushed out. The survey will be sent as a notification only if the app is pre-installed by the user. 

![5](Images/5.png)

As soon as the excel is received by the bot, it shows format of the questions extracted from the excel and upon verification, you can publish the survey 

![6](Images/6.png)



Scenario 3: End users receiving the survey

When you publish the survey, the users who have been mentioned in the excel sheet, will receive a notification and a rich card with the survey that they should fill and submit 

![7](Images/7.png)


Scenario 4: Downloading the results of a survey

Upon the admin interacting again with the app by invoking 'Hi', the option to download the survey is made available. 

The link gets an excel file with the results collated which can then be used to create a Power BI

![8](Images/8.png)

Scenario 5: Sending Reminder

The app checks those who have not filled the survey and will send reminders to them

![9](Images/9.png)








