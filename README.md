`This repository is ARCHIVED and is not maintained anymore.`

For the latest code, pls visit this repo instead:
https://github.com/asadullahrifat89/honk-buster-game-uno-platform

# Honk-Trooper

An isometric arcade bomber shooter game built with Uno Platform & WebAssembly.

Play here: https://asadullahrifat89.github.io/honk-trooper-uno-platform/

## Game Engine

The game engine has four main classes.

### Construct

This is used to define a game element. This can be added to a Scene. A construct can contain two basic functions. Animate and Recycle. These functions can be set in code.

### Generator

This is used to define a generation sequence of a particular construct. This can be added to a Scene. This accepts two functions. Startup and Generate. The Startup function is executed as soon as a generator is added to a scene.

### Scene

This is used to host and render game elements in view. It will execute each construct's Animate and Recycle function that has been added to it. Additionally, it will also execute the Generate function defined in the code.

### Controller

This is used to intercept user input from the keyboard and touch-screen. It exposes properties that can be used to detect player movement directions.

## Screenshots

![1](https://user-images.githubusercontent.com/25480176/227133683-c3324fa9-3576-4b7e-a310-61852cea83f4.png)
![2](https://user-images.githubusercontent.com/25480176/227133696-260e2104-655b-4cc1-a275-d1a4a504e2f6.png)
![3](https://user-images.githubusercontent.com/25480176/227133713-345aa366-9506-488b-8a55-5fae91a218d3.png)
![4](https://user-images.githubusercontent.com/25480176/227133719-b68d4353-1cb0-434f-99be-0498349c11ee.png)
