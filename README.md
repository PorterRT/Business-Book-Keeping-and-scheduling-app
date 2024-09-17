# Vendor-Business-Book-Keeping-and-scheduling-app
This app makes bookkeeping easier for vendors and stands through on iOS and Android.

Feature 1: Write able Card and Cash tabs, and breakdown, ie principal, tip amount, and total
  Feature 1.1 Pull directly from square (priority) or stripe account to populate credit data.
  Feature 1.2 customizable tax rate and further break down such as expenses, profit, business cost (like vendor fees)
  
Feature 2: Calendar with reminders and the ability to compare days (cost, profit, and so on)
  Feature 2.1 ability to take notes and navigate through notes and reminders for months, days, weeks

Feature 3: Vendor Performance tracking
  feature 3.1 Set and track personal goals, such as revenue or profit targets.
  feature 3.2 Give insights into analytics sucb as gender, peak time, peak days
  feature 3.3 Compare across days and months for further analytic insights for vendor
  
Feature 4: Vendor Network to talk with other vendors about events, what is happening in your area, and who/how to book a vendor stand.

we need to plan for with our database, multiple users, o auth through google, apple, and square. we will then save the usernames and give them a auto generated id so they can securely access only their own data
I will also need to make a ERD Diagram for the database Right now for feature 1 we will need a Table named "Transactions" that stores  ID (Auto Increment), Date, PaymentType, each transaction amount, the breakdown of each transaction (these will be optional). 

Note for later will need to add a try catch for if nothing is selected in payment type, and if nothing is entered in the transaction amount. 
Note will need to add more crud operations for the transactions. update still needed
Note Delete added will need to restyle all list elements to fix on phone make delete slide maybe.
Repositories folder will make it easy to go to cloud base databases instead of local SQLite databases.


  
  
  
