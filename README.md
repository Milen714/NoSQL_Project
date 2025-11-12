## Test User Account
- testserviceemployee@mail.com  secret 123
- To test the password reset functionality, an account with a real email must be created via the testserviceemployee@mail.com account.


Individual assignments 

- Hunter Pandt (726523)
* Filtering: A preloaded list of (incident) tickets can be filtered using (common) keywords. This 
is different from the search functionality because you will filter an already loaded list.  

- Milen Savov (580601)
## Milen's Individual functionalities:
1. Forget Password-functionality:
   * Users can click on `Forgot password?` located on the login page and go through the flow of resetting their password.
   * The functionality is fully operational and doesn't just display a fake email inbox view and subsequent password change-related views. My recommendation for testing the functionality is to create an account via the test "admin" account with a real email address at which you can receive emails and test the password reset.
   * How does it work?
       * The user enters their email, and the account controller checks that the account associated with that email address exists in the database.
       * The `$AccountController` calls `$GeneratePasswordResetTokenAsync`, which generates a random token that gets Base64 converted so it is suitable for use in a URL. This method also updates the user document in the DB with the reset token and 1 hr expiry time for the same token.
       * `$AccountController` builds the Callback URL (what you click in the body of the email to reset the password) and sends the email via `$GmailSenderService`
       * Once the user checks their inbox for the reset link and clicks it, the `$AccountController` `$ResetPassword(string userId, string token)` gets called via the link containing the userId and the reset token.
       * This method searches the DB for the user with the corresponding Id and compares the token provided in the callback URL with the one stored in the user document, as well as checking  if the token has expired.
       * Once all of the checks pass, the user is presented with the view where they can enter a new password and confirm it.
       * The document of the user with the corresponding ID gets updated in the DB, and the process is complete.
   *Generating and storing a secure random token unique for every user and every password reset request makes sure that the system is secure. An attacker cannot simply reset someone's password by knowing their `$ObjectId` alone.
   The SMTP settings for the Gmail account used for this project are stored together with the MongoDB connection string in the `$.env` file.

 2. Archiving the entire database (For example, all tickets older than 2 years):
    * In the `$All Tickets` view, accessible for Service employees, the user can click on the `$Show awaiting for archival` button to see tickets that have been created at least two years ago and choose to archive them in a secondary archival database. (The little notification bubble on the button shows how many incidents are viable to be archived)
    * Once the `$Archive Incidents` button is pressed, the viable for archive tickets get written to the archive database in the same MongoDB cluster and deleted from the source database(as per the assignment requirements saying the data has to be "moved").
-Dylan Mohlen (675311)
## Overview
I implemented two separate functionalities using MongoDB aggregation pipelines:
1. Advanced search with AND/OR operators
2. Bidirectional priority-based sorting

## Design Choices

### Choice 1: Separate Classes
**Decision:** Created dedicated repository and service classes
**Reasoning:** Ensures no interference with existing filtering functionality and follows SOLID principles
**Alternatives Considered:** Adding methods to existing classes (rejected to avoid coupling)

### Choice 2: MongoDB Aggregation Pipelines
**Decision:** Used `$match`, `$addFields`, `$switch`, `$sort` aggregation stages
**Reasoning:** Optimal database-level performance, reduces data transfer

### Choice 3: Interface-Based Dependency Injection
**Decision:** Created interfaces for all repositories and services
**Reasoning:** Follows existing project pattern, improves testability

### Search Functionality
- **AND Operator:** All terms must appear in subject OR description
- **OR Operator:** Any term can appear (delimiter: `|`)
- **Case-Insensitive:** Uses MongoDB regex with `$options: "i"`
- **Most Recent First:** Sorted by `reported_at` descending
- **Filter Integration:** Respects status, type, and branch filters

### Sort Functionality
- **Priority Mapping:** critical=0, high=1, medium=2, low=3
- **Bidirectional:** Critical→Low OR Low→Critical
- **Secondary Sort:** Within same priority, newest incidents first
- **Custom Field:** Uses `$addFields` to create temporary `priority_order` field

## Code Structure

### New Files Created:
1. `Models/Enums/SearchOperator.cs`
2. `Repositories/Interfaces/IIncidentSearchRepository.cs`
3. `Repositories/Interfaces/IIncidentSortRepository.cs`
4. `Repositories/IncidentSearchRepository.cs`
5. `Repositories/IncidentSortRepository.cs`
6. `Services/Interfaces/IIncidentSearchService.cs`
7. `Services/Interfaces/IIncidentSortService.cs`
8. `Services/IncidentSearchService.cs`
9. `Services/IncidentSortService.cs`

## Conclusion
This implementation demonstrates advanced MongoDB aggregation pipeline usage while maintaining clean separation of concerns and professional code quality.



## Melissa Lemus (729185)
Functionality: Transfer ticket to another person:

### 1-IncidentDetail view 
The transfer ticket to another person begins when the user presses the button for it. In the "IncidentDetail" view, a validation is implemented so if the array of assignees in the database "AssignedTo" is empty or doesn't exist, the button will show as "Assign Incident", otherwise as "Transfer Incident" because there's already an assignation active.

**Assigned_to was initially a referencing field to only one user object with a snapshot of its data. But later on, I decided to make it into an array of these snapshots in order to create a history of the assignations that a ticket might have, not just the current user working on it. 

**The assignation of the ticket is treated as a transference from no one to the user, therefore the same method is used in the controller for both actions

### 2-IncidentController
-GET Method TransferIncident: getting the users for transfer through _incidentService.GetUsersForTransferAsync and redirecting it to the TransferIncident View

### 3-InicidentService 
-The method GetUsersForTransferAsync called from the controller does not contain any logic, just a direct call to the repository

### 4-IncidentRepository: 
-The method GetUsersForTransferAsync builds the pipeline for getting all the users available for receiving a ticket transfer

### The pipeline:
$unwind: to treat every assignation of the assigned_to array as a separate element

$match: now we select only the assignations where is_active is true (a person is currently working on it) because the array is also composed of inactive (past) assignations 

$group: these active assignments are grouped by userID, and it is shown their total amount of incidents, first name and last name

$match: to filter on TotalIncidents that only users with 5 active incidents or fewer can receive a transference

**Initial code for the pipeline did not include this match stage but after consideration, I added it to relate to real-life ticketing systems, otherwise, the incidents for a user could grow endlessly.

$sort: a final stage to sort the users' names by alphabetical order to improve the aspect of the list

-Finally, the result of the pipeline will be returned as a list of user DTO (the original Assignee object included many unnecessary fields for showing in the view)

### 5-TransferIncident
The user can now see the other available users for transfer, select only one of them and in the same view (TransferIncident), the text box to include a message 

**Initial versions of the code did not include the message or all the methods related to it. It was implemented later to improve the transfer history as a way to record the reasons for it, a direct message for the new user taking up the incident or anything else that they might want to leave as "proof". 
**The database and validation rules were changed to add this new field, and in the Assignee Snapshot object, they were added as well, marking the message field as nullable because it is not mandatory, a user can make an incident transfer without it.

### 6-IncidentController
POST TransferIncident method will call the service to actually transfer the incident, redirect to the IncidentDetails with the changed parameters if everything went well, otherwise it will show an error message

### 7-IncidentService 
-Method TransferIncidentAsync calls the repository first to transfer the ticket and then to add the message AddTransferMessage. 
-AddTransferMessage passed the message and the "old user" because, as the transfer operation occurs first, if we try to access the active assignee directly through the Incident object, what we would get is the new assignee after the transfer is successful and not the "old" user that actually sent the message.  

**I tried as a variant to invert the method order and transfer the user later, but in the case of an error with the transference, the message will be added anyway because it will occur first, leading to a very high inconsistency between the result of these operations

### 8-Incident Repository
Method TransferIncidentAsync: an update method that:

-Deactivates the current assignee (sets the field is_active to false wherever is true at the moment)
**only happens if there is an active assignment to a user of this incident

-Creates the snapshot of the new assignee and adds it to the array (Update + Push operation) with an is_active field set to true

**Initial versions of this method implemented a Select to read each element of the array, change the is_active field to false and return them in a list, then added the new assignee and replaced the whole array in MongoDB. But after some considerations, I implemented the current version because it is simpler, more efficient and adjusted to the needs of the program. In the current scope of this functionality, there are no complex validations or logic applied to the assignee and only one field is changed (is_active), which can be successfully handled by the current method. 

### 9-Incident Repository 
Method AddTransferMessage: an update method on the field message of Assigned To

### 10-DisplayTransferHistory 
-Simple view with a table showing the date of the incident assignment, the user's name and last name and the message that they left before transferring it. 

-Uses: method DisplayTransfeHistory in IncidentController, GetTransferHistory in IncidentService and GetTransferHistory in IncidentRepository

-GetTransferHistory (repository method) finds the incident and returns a list of all assignations that will be displayed by the view.
