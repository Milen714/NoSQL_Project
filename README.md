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
