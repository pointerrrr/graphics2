Students:
Gideon Ogilvie      5936373
Lennart van Koot    5923395
Mike Knoop          5853915

A small description of our raytracer:
When you start the raytracer, you will see a scene consisting of: 5 spheres (one red, one textured and one reflective, one refractive and one with specular shading), 1 plane (textured, partially reflective), 6 triangles ( textured, in the shape of a pyramid).
There are 4 lights: 3 regular lights, 1 spotlight.
On the debug view we have drawn 4 string with information about the raytracer (they are self explanatory)
Do note that the debug view only works properly when the camera direction is looking straight ahead (the Y component of the direction vector is 0, the X and Z can be anything). 

You can control the camera with the following keys (you have to hold them for at least 1 frame to work, might take some time (especially if aa is enabled):
*Moving:*
Forward: W
Backward: S
Left: A
Right: D
Up: X
Down: Z
*Rotating:*
Up: Up-arrow
Down: Down-arrow
Left: Left-arrow
Right: Right-arrow
*Other controls:*
Enable AA: T
Disable AA: Y
Draw smoothly: Space
Increase FOV: P
Decrease FOV: O
Increase recursion depth: Comma
Decrease recursion depth: Period
Screenshot: I (located in My Pictures)

Implemented bonus assignments:
1. Triangle support             ( we added a pyramid to show this)
2. Spotlights                   ( the spotlight is aimed directly at the pyramid. Most of the spotlights light is caught by the pyramid, but some leaks past the sides)
3. Anti-aliasing                ( to enable this, hold T for at least 1 frame. To disable it again: hold Y )
4. Textures on all primitives   ( the floor plane has a checkerboard pattern, the middle sphere has a skybox as texture and the triangles have a brick texture)
5. Specular shading             ( the sphere in the white has specular shading)
6. A skydome                    ( one of the skydomes of http://www.pauldebevec.com/Probes/ has been implemented)
7. Refraction                   ( the first sphere on the right has a refraction index of 1.3)

Sources (if i forgot something, there is a chance it is listed as a source in the comments at the appropriate place):
ray - sphere intersection: adaptation from the slides
texture on sphere: http://www.pauldebevec.com/Probes/
ray - plane intersection: http://cmichel.io/howto-raytracer-ray-plane-intersection-theory/
ray - triangle intersection: https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
skybox implementation: http://www.pauldebevec.com/Probes/
Sphere and skybox texture: http://www.pauldebevec.com/Probes/
pyramid texture: http://www.geschichteinchronologie.com/welt/arch-Scott-Onstott-ENGL/ph01-protocol/008-012-great-pyramid-Giza-864-Heliopolis-d/007-interior-stones-great-pyramid.jpg
refraction from: https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel
A whole lot of the vector math was taken from the slides.