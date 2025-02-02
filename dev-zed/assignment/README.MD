## ZIGBANG ZED Assignment 해결 방법 

# Q1)

파일에서 데이터를 불러오기 위해 JsonUtility.FromJson를 사용하였고, DataClass.cs에 만든 struct를 사용하여 데이터를 파싱하였습니다. 질문에서 언급된 함수 (Convert.FromBase64String, Buffer.BlockCopy)를 사용하여 Base64string을 vector로 변환하였습니다.

```
                   // Convert Base64 coordinates to float. 
                    byte[] bytes = System.Convert.FromBase64String(vertex);
                    float[] floats = new float[bytes.Length / 4];
 
                    //blockcopy byte array to float array 
                    System.Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
```
Float 배열을 Vector3 배열로 변환하였고, 루프 안에서 정점들을 조회하면서  1, 2 offset을 더하여 mesh를 위한 triangle을 얻었습니다.
 
 ```
                     // Convert list of floats to array of vertices
                    for (var k = 0; k < floats.Length; k += 3)
                    {
                        vertices.Add(new Vector3(floats[k], floats[k + 2], floats[k + 1]));
                    }

                    // Get triangles for mesh.
                    for (var l = 0; l < vertices.Count; l += 3)
                    {
                        triangles.Add(l);
                        triangles.Add(l + 1);
                        triangles.Add(l + 2);
                    }
```
그리고나서, GenerateMesh(GameObject, Vector3[], int[], Material)를 호출하였습니다. 
그러나, 두 번째 질문에서 어떻게 UV mapping을 하나의 mesh에 적용할지가 고민되었습니다.

# Q2)

각각의 건물들에 주어진 texture를 이용하여 새 shader material을 만들었습니다.

```
    Shader shader = Shader.Find("Universal Render Pipeline/Lit");

    Material material = new Material(shader) {mainTexture = textureMap};
 ```

recalculatingnormals와 mesh vertices 설정 후, quad가 바라보는 각도를 찾아내야 했습니다.
앞, 위/아래, 나머지의 세 면이 있었습니다.
 
Normal과 forward(앞) 벡터 사이의 각도를 찾아서 방향을 찾았습니다
위/아래의 경우, normal과 Vector3.up || Vector3.down를 비교하였습니다.
 
Texture가 mesh를 따라 늘어나지 않도록 만들고 싶었습니다. 또한, 높이가 다른 연결된 건물들도 있었습니다. 따라서, 하나의 plane에 맵핑을 하게 되면 잘려나가는 texture가 있을 수 있어 옳지 않다고 생각했습니다. (1, 2)
 
Quad의 y와 y의 최대값을 비교한 뒤, 올바르게 UP map 좌표에 적용할 수 있도록 값을 normalize할 수 있었습니다. (3,4)

![ProblemSolution](https://imgur.com/viq6UuC.png)

Texture 맵에서의 normalize된 포지션을 얻기 위해, UV는 반복문 안에서 정점들을 조회하고 전체 정점 배열 중 최소값, 최대값과 비교하며 맵핑되었습니다. 적절한 texture를 얻기 위해 0.0-0.75의 offset을 더하였습니다.

```
  ProcessUV(Mesh);
```


# 결과 
![results](https://imgur.com/sxkkBIO.png)


# Q1
I used JsonUtility.FromJson to get the data from the file and parsed the data using structs made in DataClass.cs

I converted the Base64string to vector using the functions mentioned in the question (Convert.FromBase64String, Buffer.BlockCopy)
```
                   // Convert Base64 coordinates to float.

                    byte[] bytes = System.Convert.FromBase64String(vertex);

                    float[] floats = new float[bytes.Length / 4];


                    //blockcopy byte array to float array

                    System.Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
```

I converted the float array to a Vector3 array and got the triangles by looping through the vertices length and adding an offset of 1 and 2 to get triangles for the mesh. 
```

                    // Get triangles for mesh.

                    for (var l = 0; l < vertices.Count; l += 3)

                    {

                        triangles.Add(l);

                        triangles.Add(l + 1);

                        triangles.Add(l + 2);

                    }

```
I then called GenerateMesh(GameObject, Vector3[]., int[], Material)

I was worried about how I was going to apply UV mapping to one mesh in question 2. I made it into one mesh instead of breaking it up for optimization.

# Q2.

For each building I created a new shader material with the supplied texture 

```
Shader shader = Shader.Find("Universal Render Pipeline/Lit");


Material material = new Material(shader) {mainTexture = textureMap};
```

After recalculatingnormals() and setting the mesh vertices, I had to find out the angle the quad is facing for the UV Mapping. There are 3 sides which are Front, Top/Bottom, Else. 
```
        Vector3 normal = normals[i];

        float angle = Vector3.Angle(normal, Vector3.forward) + 180.0f;
```

I found the direction by finding the angle in between the normal and forward vector and added 180 to make a complete angle. For the top and bottom, i just compared the normal to Vector3.up || Vector3.down

I didn’t want the texture to stretch across the mesh because it would not look good. I also noticed that there are connecting buildings with different height values so mapping them to one plane wouldn’t look right as they might have texture that is cut off.

After comparing the quad’s y to max y, I could normalize that value to apply the proper UV map coordinates


The uv was mapped by  looping through the vertices and comparing it to the min and max values of the entire vertices array to get a normalized position for the texture map. Added an offset of 0.0 - 0.75 to get the according texture.

```
  ProcessUV(Mesh);
```



