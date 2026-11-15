using System.Collections.Generic;
using UnityEngine;

namespace Portfolio.EditorTools
{
    /// <summary>Génération des maillages : tout le décor est fabriqué ici, sans modèle 3D importé.</summary>
    public static class MeshLib
    {
        /// <summary>Boîte aux arêtes chanfreinées, normales lissées : les arêtes accrochent la lumière.</summary>
        public static Mesh RoundedBox(Vector3 size, float bevel)
        {
            Vector3 h = size * 0.5f;
            float b = Mathf.Clamp(bevel, 0.0005f, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * 0.95f);

            var vertices = new Vector3[24];
            var normals = new Vector3[24];
            for (int c = 0; c < 8; c++)
            {
                float sx = (c & 1) != 0 ? 1f : -1f, sy = (c & 2) != 0 ? 1f : -1f, sz = (c & 4) != 0 ? 1f : -1f;
                vertices[c * 3 + 0] = new Vector3(sx * h.x, sy * (h.y - b), sz * (h.z - b));
                normals[c * 3 + 0] = new Vector3(sx, 0f, 0f);
                vertices[c * 3 + 1] = new Vector3(sx * (h.x - b), sy * h.y, sz * (h.z - b));
                normals[c * 3 + 1] = new Vector3(0f, sy, 0f);
                vertices[c * 3 + 2] = new Vector3(sx * (h.x - b), sy * (h.y - b), sz * h.z);
                normals[c * 3 + 2] = new Vector3(0f, 0f, sz);
            }

            var triangles = new List<int>();
            int[,] loop = { { 0, 0 }, { 1, 0 }, { 1, 1 }, { 0, 1 } };

            // Six faces.
            for (int a = 0; a < 3; a++)
            {
                int u = (a + 1) % 3, w = (a + 2) % 3;
                for (int s = 0; s < 2; s++)
                {
                    var q = new int[4];
                    for (int i = 0; i < 4; i++)
                        q[i] = ((s << a) | (loop[i, 0] << u) | (loop[i, 1] << w)) * 3 + a;
                    AddQuad(triangles, vertices, q[0], q[1], q[2], q[3]);
                }
            }

            // Douze chanfreins d'arête.
            for (int c0 = 0; c0 < 8; c0++)
            {
                for (int e = 0; e < 3; e++)
                {
                    if ((c0 & (1 << e)) != 0) continue;
                    int c1 = c0 | (1 << e);
                    int a1 = (e + 1) % 3, a2 = (e + 2) % 3;
                    AddQuad(triangles, vertices, c0 * 3 + a1, c1 * 3 + a1, c1 * 3 + a2, c0 * 3 + a2);
                }
            }

            // Huit coins.
            for (int c = 0; c < 8; c++) AddTriangle(triangles, vertices, c * 3, c * 3 + 1, c * 3 + 2);

            var mesh = new Mesh { vertices = vertices, normals = normals, uv = new Vector2[24] };
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        static void AddQuad(List<int> triangles, Vector3[] v, int a, int b, int c, int d)
        {
            AddTriangle(triangles, v, a, b, c);
            AddTriangle(triangles, v, a, c, d);
        }

        // Le solide est convexe et centré : on oriente chaque triangle vers l'extérieur.
        static void AddTriangle(List<int> triangles, Vector3[] v, int a, int b, int c)
        {
            Vector3 normal = Vector3.Cross(v[b] - v[a], v[c] - v[a]);
            Vector3 centroid = (v[a] + v[b] + v[c]) / 3f;
            if (Vector3.Dot(normal, centroid) < 0f)
            {
                int swap = b;
                b = c;
                c = swap;
            }
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        /// <summary>Cylindre posé sur sa base (y = 0), axe vertical.</summary>
        public static Mesh Cylinder(float radius, float height, int segments)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                var radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices.Add(radial * radius);
                normals.Add(radial);
                vertices.Add(radial * radius + Vector3.up * height);
                normals.Add(radial);
            }
            for (int i = 0; i < segments; i++)
            {
                int a = i * 2, b = ((i + 1) % segments) * 2;
                triangles.AddRange(new[] { a, a + 1, b + 1, a, b + 1, b });
            }

            for (int cap = 0; cap < 2; cap++)
            {
                float y = cap == 0 ? 0f : height;
                Vector3 normal = cap == 0 ? Vector3.down : Vector3.up;
                int center = vertices.Count;
                vertices.Add(new Vector3(0f, y, 0f));
                normals.Add(normal);
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius));
                    normals.Add(normal);
                }
                for (int i = 0; i < segments; i++)
                {
                    int a = center + 1 + i, b = center + 1 + (i + 1) % segments;
                    if (cap == 0) triangles.AddRange(new[] { center, a, b });
                    else triangles.AddRange(new[] { center, b, a });
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, new Vector2[vertices.Count]);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Brique à tenons, origine au centre de sa base.</summary>
        public static Mesh Brick(int width, int depth, float height, float stud)
        {
            var parts = new List<CombineInstance>();
            Mesh body = RoundedBox(new Vector3(width * stud - 0.003f, height, depth * stud - 0.003f), 0.005f);
            parts.Add(new CombineInstance { mesh = body, transform = Matrix4x4.Translate(new Vector3(0f, height * 0.5f, 0f)) });

            Mesh knob = Cylinder(stud * 0.3f, stud * 0.21f, 12);
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    var offset = new Vector3((x + 0.5f - width * 0.5f) * stud, height - 0.001f, (z + 0.5f - depth * 0.5f) * stud);
                    parts.Add(new CombineInstance { mesh = knob, transform = Matrix4x4.Translate(offset) });
                }
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(parts.ToArray(), true, true);
            mesh.RecalculateBounds();
            Object.DestroyImmediate(body);
            Object.DestroyImmediate(knob);
            return mesh;
        }

        /// <summary>Plan vertical tourné vers -Z, avec coordonnées de texture (écrans, ciel).</summary>
        public static Mesh Quad(float width, float height)
        {
            float x = width * 0.5f, y = height * 0.5f;
            var mesh = new Mesh
            {
                vertices = new[] { new Vector3(-x, -y, 0f), new Vector3(x, -y, 0f), new Vector3(x, y, 0f), new Vector3(-x, y, 0f) },
                normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back },
                uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) },
                triangles = new[] { 0, 3, 2, 0, 2, 1 }
            };
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Nappes de lumière qui partent d'une fenêtre et suivent la direction du soleil jusqu'au sol.</summary>
        public static Mesh LightShafts(float xMin, float xMax, float yMin, float yMax, float z, Vector3 direction, int sheets)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            direction.Normalize();

            for (int i = 0; i < sheets; i++)
            {
                float y = Mathf.Lerp(yMin, yMax, (i + 0.5f) / sheets);
                float travel = y / Mathf.Max(-direction.y, 0.05f);
                Vector3 drop = direction * travel;
                int start = vertices.Count;
                vertices.Add(new Vector3(xMin, y, z));
                vertices.Add(new Vector3(xMax, y, z));
                vertices.Add(new Vector3(xMax, y, z) + drop);
                vertices.Add(new Vector3(xMin, y, z) + drop);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));
                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
