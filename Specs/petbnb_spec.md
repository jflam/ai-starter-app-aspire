Follow these instructions to Create an Airbnb‑style marketplace that connects pet owners with verified sitters. Use this sketch as the primary layout and infer any missing components. The first feature displays potential matching sitters for both logged out and logged in users.
 
## General instructions
The following are general instructions on sections in the spec that we are creating. Assume that the spec that currently is in context in this chat is the one that the user is working on.
 
## Spec description
Below are instructions for what you should do in each part of the spec.
## Overview
An overview of the application we are building. When significant changes happen in the spec, make sure that you update this section.
## Competitive Landscape
This section contains (if appropriate) examples of existing prior art in this space to guide in the creation of our application. If the user researches competitors, put our research for each competitor in a separate sub-section of each document. If the user asks follow-up questions for that competitor, update that section.
## Features
This section contains a description of individual features. A feature is something intended to be spec'd in full ahead of coding. So a feature section ideally contains the following sub-sections:
### Motivation
Why does this feature exist? What customer problem does it solve? 
### User Stories
Narratives that describe how the feature is to be used. When the user starts specifying a new feature make sure to record the user stories here. Make sure to check to see if the user stories are out of sync if the user changes requirements on the feature and update when appropriate.
### Requirements
This is a detailed list of requirements for the feature that are extracted from the user stories. This is the "what" of the feature and will be used by planning and coding agents to plan the step by step implementation plan and to implement the feature.
### Acceptance Criteria
This is a detailed description of how we know when the feature is done. This should be written up as a set of Gherkin acceptance scenarios. 
### Testing 
This is a list of detailed Playwright tests that must pass. During testing we will be driving Playwright using an MCP server that automates the interactions.