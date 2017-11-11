# Requirements Analysis
## Software Description
Pleisure is a web platform which connects parents with activities providers for their children. It aims to offer parents the capability of finding high quality activities for their siblings via a simple and easy to use user interface, and at the same time, provide a handy environment for corresponding services and businesses for advertising and reaching out to more potential customers.
## Functional Requirements
### User Groups and corresponding operations
The users of the platform can belong to one of the following groups:

1. Anonymous users
2. Parents
3. Vendors
4. Administrators

Anonymous users have access to all the content of the platform. They can view, without the need to login, all the events that are available, with priority to those hosted near them (if they choose to share their location). They can also apply filters, to limit the events displayed and adjust the search to their preferences.

Parents are the users that are logged in as such. They can perform all the actions that an Anonymous user can, plus they have access to a personal profile page where they can add information and photos, and can also buy credits (see [Monetization](#monetization)). In this personal page there is also an option to "add child", which if clicked, a parent can add information about his child (such as age and prefered activity), so that the events displayed in the search results will have pre-applied filters accordingly.

Vendors are the activity-events providers. They can perform all the actions that Anonymous users can too, and also create events. Their "profile page" is similar to this of the "parent" user, but instead of "add child" option, they have the "add event" option, where they can fill out a special form with details to publish the event.

Administrators are special users that can manage the other 3 types of users. They can block users and change their passwords.

Authentication of the users is performed via their passwords. 
### Entry Points
All the users visiting the web platform, can immediately utilise the functionality of the service, by typing keywords in the main search bar. The search results are displayed on another page, to which the user is redirected after performing the search query.c
### Monetization
The platform uses an internal currency system. 

More specifically, different types of users can interact financially with each other by using "credits". "Parents" can buy credits via PayPal, and use these tokens to pay the "Vendors" for the chosen activities. The amount of credits is bound to the user's account, and no further kind of transactions are supported.

#### Platform
The website's owner can profit by keeping a commission for every transaction fulfilled.
### Legal
#### Software license
MIT License

Copyright (c) [2017] [Pleisure]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

#### Cookies Policy
Users can instruct their browsers to refuse all cookies or to indicate when a cookie is being sent. However, if they do not accept cookies, they will not be able to use some portions of the service.

## Non-functional Requirements
Recovery ability-Fault Tolerance
Usability-Reliability
Security-Safety-Integrity
Scalability
Robustness
Flexibility-Extensibility
Responsiveness
Performance-Efficiency
ISO-Standards Compliance
Interoperability-Interfacing with other software