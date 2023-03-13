# Honk-Trooper

An  isometric bomber shooter game where your goal is to bomb as many honking cars as possible.

Play here: https://asadullahrifat89.github.io/honk-trooper-uno-platform/

## Game Engine

The game engine has four main classes.

### Construct

This is used to define a game element. This can be added to a Scene. A construct can contain two basic functions. Animate and Recycle. These functions can set in code.

### Generator

This is used to define a generaion sequence of a particular construct. This can be added to a Scene. This accepts two functions. Startup and Generate. The Startup function is executed as soon as a generator is added to a scene.

### Scene

This is used to host and render game elements in view. It will execute each constructs Animate and Recycle function that has been added to it. Additionally it will also execute the Generate function defined in code.

### Controller

This is used to intercept user input from keyboard and touch-screen. It exposes properties which can be used to detect player movement directions.

## Screenshots

![localhost_5000_(iPad Mini)](https://user-images.githubusercontent.com/25480176/224575848-22fc9fca-7938-4d14-b7df-9d68dc97e234.png)
![localhost_5000_(iPad Mini)2](https://user-images.githubusercontent.com/25480176/224575851-56d698a7-33aa-4512-9697-654376852cb1.png)
![localhost_5000_(iPad Mini)3](https://user-images.githubusercontent.com/25480176/224575855-1054284e-712b-452d-9a46-9ec8205fc73d.png)
![localhost_5000_(iPad Mini)4](https://user-images.githubusercontent.com/25480176/224575858-5c968d73-dd2d-4d88-acca-95bdc02f8ca5.png)
![localhost_5000_(iPad Mini)5](https://user-images.githubusercontent.com/25480176/224575866-22f5d5fa-b5ba-47ce-9b7a-c433e8bc802d.png)
