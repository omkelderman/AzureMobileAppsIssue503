# Azure Mobile Apps - Bug Showcase #503
This project exists for the sole purpose of showing the problem described in https://github.com/Azure/azure-mobile-apps/issues/503


## Instructions on how to run and explaination of the bug
1. Run the `Server` project, it expects a connection string in its app settings with name `DefaultConnection` pointing to an azure cosmos db.	
   - Do *not* create the container, this app will do it to get it in the correct state where the bug shows up.
   - When started a new container will be created, note how this one does not have a composite index (yet :eyes:), this is intended for this showcase.
   - The client assumes `http://localhost:8085` as server url. I was too lazy to make that configurable. Make sure the server runs on that port :D
2. Run the `Client` project with commandline argument "fill" (eg `dotnet run fill`). This will add some data to the server as we need some data for the bug to show up.
3. Now its showtime: Run the `Client` project again but now with the commandline argument "fetch".
   - This will run the get request `http://localhost:8085/tables/todoitem?$orderby=updatedAt&$count=true`. This is the same as what the offline table `PullItemsAsync` would do and is what triggered this whole story.
   - Note how this will run literally forever in an endless loop. The only way to kill it is to kill the process.
   - Meanwhile looking at the server console we can see it spams requests like crazy repeating the same request over and over and we can also see a bunch of cosmos errors.
   - message inside the cosmos error:
     > The order by query does not have a corresponding composite index that it can be served from.
   - JSON response of the request:
     ```json
     {
       "items": [],
       "count": 5,
       "nextLink": "http://localhost:8085/tables/todoitem?$orderby=updatedAt&$count=true"
     }
     ```
   - Note how this response makes no sense for 2 reasons:
     1. It should contain 5 items in its item array, but it has none and the `nextLink` property is filled with a url that is itself, enabling the endless loop part
     2. Cosmos gives a pretty clear error, there is an exception, it talks about a BadRequest (400), yet for some reason this error is not reflected in the response at all, making it not obvious *at all* that something went wrong, except for a pretty weird response.
   - **This here is what I consider the actual the bug, that this request does not error out despite cosmos erroring out very clearly**
4. Now lets run the `Client` project again but this time with commandline argument "fix-index". This will send a custom request to the server where an extra controller has code that will insert the needed composite index into the cosmos container
5. Finally if we repeat the "fetch" call again, this time it will succeed as expected
   ```json
   {
     "items": [
       {
         "title": "Todo Item #0",
         "isComplete": false,
         "id": "35d98cf6839b413eb28127f70215bb88",
         "updatedAt": "2023-06-09T18:03:32.789+00:00",
         "version": "IjBlMDA3NjRkLTAwMDAtMGQwMC0wMDAwLTY0ODM2OTc1MDAwMCI=",
         "deleted": false
       },
       {
         "title": "Todo Item #1",
         "isComplete": false,
         "id": "1d46f8d7cd9445909094684244913b87",
         "updatedAt": "2023-06-09T18:03:33.272+00:00",
         "version": "IjBlMDA3NzRkLTAwMDAtMGQwMC0wMDAwLTY0ODM2OTc1MDAwMCI=",
         "deleted": false
       },
       {
         "title": "Todo Item #2",
         "isComplete": false,
         "id": "63b5c91163624662a9d2cea228c8609c",
         "updatedAt": "2023-06-09T18:03:33.301+00:00",
         "version": "IjBlMDA3ODRkLTAwMDAtMGQwMC0wMDAwLTY0ODM2OTc1MDAwMCI=",
         "deleted": false
       },
       {
         "title": "Todo Item #3",
         "isComplete": false,
         "id": "1f5f3dc8cf754e7d839db1ee310d9f6b",
         "updatedAt": "2023-06-09T18:03:33.321+00:00",
         "version": "IjBlMDA3OTRkLTAwMDAtMGQwMC0wMDAwLTY0ODM2OTc1MDAwMCI=",
         "deleted": false
       },
       {
         "title": "Todo Item #4",
         "isComplete": false,
         "id": "c376d4cb248e470f8690cf8c93df2bb2",
         "updatedAt": "2023-06-09T18:03:33.342+00:00",
         "version": "IjBlMDA3YTRkLTAwMDAtMGQwMC0wMDAwLTY0ODM2OTc1MDAwMCI=",
         "deleted": false
       }
     ],
     "count": 5,
     "nextLink": null
   }
   ```
   All the entities are there in the `items` array and the `nextLink` property is `null`