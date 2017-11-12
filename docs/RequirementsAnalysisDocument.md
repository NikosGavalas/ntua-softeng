# 'Pleisure' Requirements Analysis Document
***
## Software Description
Pleisure is a web platform which connects parents with activities providers for their children. It aims to offer parents the capability of finding high quality activities for their kids via a simple and easy to use user interface, and at the same time, provide a handy environment for corresponding services and businesses for advertising and reaching out to more potential customers.
***
## Functional Requirements
### User Groups and corresponding operations
The users of the platform can belong to one of the following groups:

1. Guest users
2. Parents
3. Organizers
4. Administrators

Guest users have access to most of the content of the platform. They can view, without the need to login, all the events that are available, with priority to those hosted near them (if they choose to share their location). They can also apply filters, to limit the events displayed and adjust the search to their preferences. However, they cannot book events.

Parents are the users that are logged in as such. They can perform all the actions that a Guest user can, plus they have access to a private profile page where they can add information and photos, and can also buy credits (see [Monetization](#monetization)). In this personal page there is also an option to "add child", via which, a parent can add information about his children (such as age and prefered activity), so that the events displayed in the search results will have pre-applied filters accordingly for quicker personalization.

Organizers are the activity/events providers. They can perform all the actions that Guest users can too, and also create events. Their "profile page" is similar to this of the "parent" user, but instead of "add child" option, they have the "add event" option, where they can fill out a special form with details to publish the event. These details include the exact time and date of the event, info about it being reccuring or not, its pricing, address (geolocation), photos etc.

Both of the two aforementioned users (Parent and Organizers) can use the "report" function to mark any inappropriate activity.

Administrators are special users that can manage the registered users (Parents and Organizers). They can:
- Review the reports of the users
- Ban users, if they notice any violation of the Terms of Usage
- Restrict user functionality (such as event creation for an Organizer)
- Reset passwords

Authentication of the users (belonging to the groups Parents/Organizers/Administrators) is performed with the use of  passwords that are set on each respective signup.
### Entry Points
All the users visiting the web platform, can immediately utilise the functionality of the service by entering their address in the main textfield (or by allowing their browser to share their location so it can be automatically determined). The nearby events are then displayed on another page, to which the user is redirected after performing the query action.
### Monetization
The platform uses an internal currency system. 

More specifically, different types of users can interact financially with each other by using "credits". "Parents" can buy credits via PayPal, and use these tokens to pay the "Organizers" for the chosen activities. The amount of credits is bound to the user's account, and no further kind of transactions are supported.

Moreover, the website's owner can profit by keeping a commission for every transaction fulfilled.
### Legal
#### Software license
The software is licensed under the MIT License:

> Copyright (c) 2017 Pleisure

> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:

> The above copyright notice and this permission notice shall be included in all
> copies or substantial portions of the Software.

> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
> SOFTWARE.

#### Cookies Policy
Users can instruct their browsers to refuse all cookies or to indicate when a cookie is being sent. However, if they do not accept cookies, they will not be able to use some portions of the service.
***
## Non-functional Requirements
### Security
The software implements all the security standards to prevent SQL injection attacks, XSS, and user accounts leak from the database.
Sessions are over TLS so all communication with the server is encrypted.
The sensitive data of the users (such as information about the children of the "parent" users) are visible only to the user itself and the Administrator users.
### Usability
The User Interface is the simplest possible, so that even inexperienced users can have full experience of the functionality of the service. It features easy navigation for all user groups.

### Flexibility
The software can be deployed on any platform that can run .NET >= 4.6 or Mono >= 5.4 and a MySQL database.

### Deployment
The installation procedure (both building and deployment) is automated with the use of a script.

### Responsiveness
The UI is responsive, meaning it offers optimal viewing experience across a wide range of devices (Desktop, Tablet, Mobile).