# 'Pleisure' Requirements Analysis Document
## Software Description
Pleisure is a web platform which connects parents with activities providers for their children. It aims to offer parents the capability of finding high quality activities for their kids via a simple and easy to use user interface. At the same time, it provides a handy environment for corresponding services and businesses for advertising and reaching out to more potential customers.
## Functional Requirements
### User Groups and corresponding operations
The users of the platform can belong to one of the following groups:
1. Guest users
2. Parents
3. Organizers
4. Administrators
#### Guest users
Permissions: Guest users have access to most of the content of the platform. They can search and view available events as well as apply filters, to limit the displayed results to their preferences. 
Restrictions: Guest users do not have a profile, as a result they are not allowed to have a wallet or buy coins. Guest users cannot create or book events and send reports. They also do not have any administrator privilage.
#### Parents
Permissions: Users signed up as Parents can perform all the actions that a Guest user can. Additionally, they have access to a private profile page where they have their personal and contact information, photos, booking history and wallet with their credits  (see [Monetization](#monetization)). Parents can buy credits with any of the given payment methods, and they can use those credits to book events. In this personal page there is also an option to "add child", via which, a parent can add information about his children (such as date of birth and prefered activity), so that the events displayed in the search results will have pre-applied filters accordingly for quicker personalization. At last they can use the "report" function to mark any inappropriate activity.
Restrictions: Parents cannot create events and they do not have any administrator privilage.
#### Organizers
Permissions: Organizers are the activities/events providers. They can perform all the actions that Guest users can. In addition, they have a public "profile page" in which they have the "add event" option, where they can create an event, by supplying the necessary information. These information include the exact time and date of the event, info about it being reccuring or not, its pricing, address (geolocation), photos etc. Like parents they can use the "report" function to mark any inappropriate activity. 
Restrictions: Organizers do not use the credits system, so they do not have a wallet. As a result cannot book other events. They also do not have any administrator privilage.

#### Administrators
Permissions: Administrators are special users that can manage the registered users (Parents and Organizers). They can:
- Review the reports of the users
- Ban users, if they notice any violation of the Terms of Usage
- Restrict user functionality (such as event creation for an Organizer)
- Reset passwords
Restrictions: They do not have access to any private information of the users, such as passwords, billing information etc.

Authentication of the users (belonging to the groups Parents/Organizers/ ??? Administrators) is performed with the use of  passwords that are set on each respective signup.
Organizers should be oblidged to provide a bank account or an IBAN since they are not using the credits system, in order to get paid for their services.

### Entry Points
All the users visiting the web platform, can immediately utilise the functionality of the service by typing in the type of activity in the main textfield.The nearby events with matching tags are then displayed on another page, to which the user is redirected after performing the query action. 
Else, by clicking the “Events” tab on the navbar they are getting redirected directly to the event page, where they can browse events and apply filters.

### Monetization
The platform uses an internal currency system regarding Parents and the booking of the event. More specifically, Parent users can buy credits from the website using their PayPal account or Bitcoin Portfollio. Those credits are secured in their account wallet, and can be used for booking events. The credits cannot be refunded back to the parents as real money, once they purchased them. The amount of credits is bound to the user's account, and no kind of transactions between users are supported.
Organizers, are paid via bank transfer or PayPal transaction since they are not using the credits system. All used credits for event booking are simply dismissed and their EUR value is paid to the event holder, minus the website’s standard commission (e.g. 100 credits). Therefore Organizers must provide to the website the necessary billing information in order to get paid for their services.

### Event Search and Results Filtering
As mentioned before, 'Pleisure' offers a simple search engine on the website's main page. This aims to enable every user, to narrow the list of given events, so that it matches their needs and prefernces. The user types in an activity and the corresponding events are shown immeditely without having to browse through all website's events.
'Pleisure' provides further result filtering on the Events page. In order to use the filtering tool a location must be provided by the user. Given that, the user limits  theevent shown results by adjusting:
-Distance from the given location
-Maximum Price of the event
-Age Range of the participants
-Activity Category
-Date of the event

## Non-functional Requirements
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
### Security
The software implements all the security standards to prevent SQL injection attacks, XSS, and user accounts leak from the database.
Sessions are over TLS so all communication with the server is encrypted.
The sensitive data of the users (such as information about the children of the "parent" users) are visible only to the user himself and the Administrator users.
### Usability
The User Interface is the simplest possible, so that even inexperienced users can have full experience of the functionality of the service. It features easy navigation for all user groups.

### Flexibility
The software can be deployed on any platform that can run .NET >= 4.6 or Mono >= 5.4 and a MySQL database.

### Deployment
The installation procedure (both building and deployment) is automated with the use of a script.

### Responsiveness
The UI is responsive, meaning it offers optimal viewing experience across a wide range of devices (Desktop, Tablet, Mobile).
