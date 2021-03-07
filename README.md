# mathlib
Geometry and linear algebra library. This library was developed using Unity3D. A lot of the code is based on Unity API. I made this library for personal use- primarily because I liked the unity math functions but wanted access to a similar library without having to use the UnityEngine versions.

Shapes: Sphere, Box, BBox, Capsule, Convex Polygons  
  - There are intersection tests & other functions for manipulating each shape.

Implemented Vector2d, Vector3d, Vector3Int, Vector2Int.   
  - Inspired by Unity3D Vectors- but implemented with double & integer precision.
  - There is also a quaternion (double precision) implementation. 
  - There is also a matrix4x4 struct implemented- and a transform struct which is essentially a struct implementation of Unity3D transforms.

There is also some space partitioning data structures.  
  - There is an Octree & sphere bins. The sphere bins is just a big 3D array that uses spheres for object bounds. The octree uses AABB for object bounds.
  - Each structure supports intersection tests with spheres, AABB, and raycasts.

SimpleRandom: Linear congruential generator
  - Works like system.random except it is a struct- so there are no allocations.  

HashGraph: A graph implementation that uses unique vertices/ (Each vertex can be hashed & have constant lookup time)  
  - Implements A star search & connected component  

Note* Some segments of the code are just copy/paste from other repos. Sources are directly in the code. (eg a lot of the quaternion and matrix manipulation code). Feel free to use my code however you want. Creative commons license https://creativecommons.org/publicdomain/zero/1.0/ for the other 99% of this repo that I have personally written.


