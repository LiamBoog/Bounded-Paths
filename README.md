# Bounded Paths

### UPM Install Link
```
https://github.com/LiamBoog/Bounded-Paths.git
```

A Unity package that enables the creation of `BoundedPath` objects, which are 2D paths with arbitrary[^1] boundaries. This contrasts other path tools which generally create paths with arbitrary centerlines but a constant width. With Bounded Paths, path boundaries may be shaped however the user likes, using splines, and the path's mesh and centerline will be updated in real-time.

Bounded Paths allow users to intuitively build paths to meet their specific needs directly in Unity without the hassle of manually defining meshes and centerlines. Below are some sample paths demonstrating the capabilities of Bounded Paths. (Note: the centerline visualization shown is not included with the package but is used to illustrate the `BoundedPath`'s automatically generated centerline.)
<p align="middle">
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/0ceb7d6f-8379-41fa-b11f-854556399bc4" width="40%"/>
  <img src"" width="2.5%"/>
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/4de5b489-fa8d-4582-9ab4-d30f37d246c3" width="34.1%"/>
</p>

[^1]: Technically, path boundaries can't actually be arbitrary and are subject to the limitations outlined in the [Limitations](#limitations) section.

## Getting Started
To get started creating your own paths,
1. Install Bounded Paths with Unity's Package Manager using the link above ([UPM Install Link](#upm-install-link)).
2. Create a `BoundedPath` directly, from the context menu, or by adding a `BoundedPath` component to an existing object.
<p align="middle">
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/4c6d1546-6b08-43f5-bdd7-efd34aa05327" width="30%"/>
  <img src"" width="2.5%"/>
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/dcc80da4-ae3b-4c3e-ad0b-9b4e2441312a" width="31.7%"/>
</p>

3. Edit the boundaries using [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.3/manual/getting-started-with-splines.html).
<p align="middle" justify="top">
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/f6fd5be8-95a3-4c5d-aee7-0b3961dda0cc" width="20%" align="top"/>
  <img src"" width="2.5%"/>
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/5b976647-89c7-4e9b-bc7f-b62fe2eb7146" width="45%"/>
</p>

## Limitations
Procedural mesh and centerline generation may not work properly in any of the following conditions:
* Either of a `BoundedPath`'s boundaries are self-intersecting.
<p align="middle">
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/b7251360-b742-45b5-8b25-d1c3612f01cb" width="30%"/>
</p>
  
* A `BoundedPath`'s boundaries intersect with one another.
<p align="middle">
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/5c65e718-3d7b-4fde-b5eb-8b4755b9e8ae" width="30%"/>
</p>

* Either of a `BoundedPath`'s boundaries has an unusual shape with protrusions/intrusions.
<p align="middle">
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/0747d315-7d7f-4c59-9adc-ed474de29e44" width="30%"/>
  <img src"" width="2.5%"/>
  <img src="https://github.com/LiamBoog/Bounded-Paths/assets/48077738/80ccbf7a-37f6-487e-b7f6-7b2e39093eb1" width="31.2%"/>
</p>

In any of these cases, the resulting `BoundedPath` no longer even resembles a path, so these limitations should not be an issue the vast majority of the time. 
