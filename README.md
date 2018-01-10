# Pleisure

## Team

|Member|AM|Development|Secondary Task|
|---|---|---|---|
|[Mantaos Grigoris](https://bitbucket.org/gmantaos/)|03113171|Back-End||
|[Gavalas Nikos](https://bitbucket.org/nickgavalas/)|03113121|Front-End Scripting|Reports|
|[Karameris markos](https://git.gmantaos.com/MarkosK)|03113148|Font-End Interface|Wireframes|
|[Tsoulias Kostas](https://bitbucket.org/Proteas94/)|03112043|Module|QA|
|[Karteris Antonis](https://git.gmantaos.com/UphillD)|03112076|Module| |

## Reports

|Report|Deadline|
|------|--------|
|[Requirement Analysis](https://bitbucket.org/teamdroptabel/pleisure/src/7cb6c296062024cafe0498d5c44a37eba3f6001c/docs/RequirementsAnalysisDocument.md?at=master&fileviewer=file-view-default)|7/1/2017|
|[Technical Specifications](https://bitbucket.org/teamdroptabel/pleisure/src/7cb6c296062024cafe0498d5c44a37eba3f6001c/docs/TechnicalSpecificationsDocument.md?at=master&fileviewer=file-view-default)|7/1/2017|

## Links

- [Documentation](https://git.gmantaos.com/ProgTech/Pleisure/wiki)
- [Project description](https://courses.softlab.ntua.gr/softeng/2017b/Project/project.pdf)
- [Updated requirements](http://courses.softlab.ntua.gr/softeng/2017b/Project/project.v2.pdf)
- [Hosting](https://progtech.gmantaos.com) (soon)
- [Slack](https://progtechteam.slack.com)

## Dependencies

Add the following to your NuGet sources: `https://nuget.gmantaos.com/api/v2/`

- [Newtonsoft.Json](https://www.newtonsoft.com/json)
	- Obviously...
- [HaathDB](https://git.gmantaos.com/Haath/HaathDB)
	- MySQL wrapper for async operations and type conversion
- [Chance.NET](https://github.com/gmantaos/Chance.NET)
	- Random data generation for populating the database and running tests
- [Watermark](https://bitbucket.org/teamdroptabel/watermark)
	- Library for watermarking the photos of organizers

## Installation

```bash
git clone https://gmantaos@bitbucket.org/teamdroptabel/pleisure.git
cd pleisure
make
```