# IDE4Arduino - An alternative Arduino IDE.

I really like the Arduino Platform to quickly hack together some function. If it is in the lab or at home, the Arduino gets things done quickly. But from time to time I was annoyed by the IDE. Of course you can setup eclipse, makefiles and so on, but I decided to program some small IDE. 

The text editing comes from the ScintillaNET project (https://scintillanet.codeplex.com/). Floating, dockable windows from DockpanelSuite (http://dockpanelsuite.com/) and the toolchain and configuration from the original Arduino IDE (http://www.arduino.cc). 

## Features
The IDE offers some additional features like: 
* code completion 
* code folding (get rid of your lengthy comments) 
* a quick select box for functions 
* double clicking an error brings you to the error (o.k. not always) 
* auto add closing brackets 
* brackets highlighting 
* Help Browser: by pressing F1 on a selected keyword the corresponding arduino page is opened (or something else, if nothing was found, dangerous: uses Internet Explorer) 
* Terminal Window: can display data in hex format 


## Build
Prerequirenments:
* ScintillaNET (https://scintillanet.codeplex.com/)
* DockPanel Suite (http://dockpanelsuite.com/)

