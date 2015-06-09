# Umbraco REST API

The Umbraco REST API is for content, media, members & relations. It's Based on the [HAL specification](http://stateless.co/hal_specification.html) and is using a wonderful WebApi implementation of HAL which can be found on GitHub: [https://github.com/JakeGinnivan/WebApi.Hal](https://github.com/JakeGinnivan/WebApi.Hal)

## Installation

*Coming soon!*

Umbraco REST API can be installed via Nuget:

    Install-Package UmbracoCms.RestApi

## Discovery

A great way to browse Umbraco's REST service is to use the great html/javascript [HAL Browser](https://github.com/mikekelly/hal-browser). The starting endpoints are:

* /umbraco/rest/v1/content
* /umbraco/rest/v1/media
* /umbraco/rest/v1/members
* /umbraco/rest/v1/relations

We will be enabling a single root endpoint that list these HAL links in the very near future!
