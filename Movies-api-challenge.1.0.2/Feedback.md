### Feedback

*Please add below any feedback you want to send to the team*

## Find Postman collection to test with request data. I have added a file next to this Feedback named `Lodgify Cinema API.postman_collection.json`.

## Summary of Development:

### 1. API Development Setup:
   - Created a .NET Core API project.
   - Configured Entity Framework Core for data access (As it provided with in-memory DB).
   - Implemented integration with an in-memory database named CinemaDB.
   - Set up Swagger for API documentation.

### 2. Entity and Controller Setup:
   - Created controller endpoints for creating showtime, reserving seats, and confirm (buy) seats.

### 3. Implemented Commands and Queries:
   - **Configuration issue**
	 - docker-compose not working somehow. I suggest to run 'Docker' profile from Visual Studio.
   - **Create Showtime:**
     - Created a showtime with validation rules and fetched movie data from the in-memory DB.
   - **Reserve Seats:**
     - Implemented seat reservation with validation rules such as avoiding double reservations, contiguous seats, and expiration after 10 minutes.
   - **Buy Seats:**
     - Enabled purchasing seats with validation for reservation expiration and duplicate purchases.
   - **Get Auditorium:**
     - Retrive the Auditorium data.
 

### 4. Integration with ProvidedApi (PENDING):
   - Tried to communication with an external gRPC API named MoviesApi using the provided `movies.proto` file.
   - Utilized gRPC API for communication with ProvidedApi using the `MoviesApiClient`.
   - Implemented methods to interact with ProvidedApi for fetching movie details.
   - NOTE: I didn't gave too long time so first I try to finish other task.


### 5. Caching Responses:
   - Integrated Redis cache layer to cache responses from the ProvidedApi for improved performance and resilience.
   - Implemented in code, but Redis connection not established somehow so code is comment out.

### 6. Logging Execution Time:
   - Implemented custom middleware to track the execution time of each request and log it to the console.

### 7. Additional Service Methods To Support Validation (region Helper):
   - Developed service methods for retrieving reservation details, checking seat availability, and handling expired reservations.

### 8. Error Handling and Validation:
   - Ensured proper error handling and validation throughout the application to maintain data integrity and provide meaningful feedback to clients.