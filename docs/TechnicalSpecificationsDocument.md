# Technical Specifications

## Architecture and Design

Language, Libraries, Frameworks used...and the reasons that we chose these over others?

The development stack has been arranged as follows:

- Database: **MySql**
	- The relational model, generally suits well any application that does not work with any Artificial Intelligence technologies, or any form of data that is otherwise sparsely defined and/or distributed. Therefore, it became apparent that the application's back-end storage would be within the SQL family. Within that set of languages, MySql was pretty straight-forward choice as it is free, flexible, easy to set up or use and widely supported in any programming language by numerous libraries and functions.
- Web-Server: **C#**
	- Although the choice here was mostly preferential, it is a strong one, as the .NET Framework is very versatile, robust and agile in development. This language is further supported by Microsoft's own Visual Studio, which makes any workflow effortlessly streamlined and optimized.
	- For any interaction between the web server and the application, the former will provide a JSON API for data retrieval, as well as any operation the user might intend to perform through the application, for example booking an event or changing their password.
- Front-End: **HTML, CSS, Javascript**
	- As this is a web application, the default W3C set of languages has been chosen to both design and render the application. Any alternatives would involve the use of specialized front-end rendering frameworks, which were deemed to be beyond the scope of the project. Describing the application natively in the web languages also ensures abstraction, making the front-end of the project not have any major dependency on any other parts of stack.


Visualizations/Modeling, flow charts, data flow charts, decision trees...



### UML diagrams

UX visualizations, control flow

### Wireframes

describing the UI

### OOP Models

also Interfacing between components etc

### Design Trade-offs

for example we assume that users have cookies available (to keep the sessions alive) 

## System Requirements

### Environment

also Dependencies...

### Hardware and Network requirements

server hardware... bandwidth?..

## Implementation

### Acceptance Tests

tests descriptions

## Deployment

how to build and deploy (automated?), suggested monitoring procedures, (possible bugs reporting?), maintenance?





