Individual assignments 

-Hunter Pandt (726523)
* Filtering: A preloaded list of (incident) ?ckets can be filtered using (common) keywords. This 
is different from the search func?onality because you will filter an already loaded list.  
* Can be located in the Incident Repository; GetAllIncidentsPerStatus, GetAllIncidentsByType& GetIncidentsByStatusAndType. Each should follow MongoDb
aggregation pipeline


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
