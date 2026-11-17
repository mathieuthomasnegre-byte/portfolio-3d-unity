using System.Collections.Generic;
using UnityEngine;

namespace Portfolio.EditorTools
{
    /// <summary>Génération des maillages du décor fabriqué par code (murs, briques, objets absents des paquets de modèles).</summary>
    public static class MeshLib
    {
        /// <summary>
        /// Boîte aux coins vraiment arrondis (rayon « radius »), façon jouet moulé : chaque face est une grille
        /// projetée sur une boîte intérieure, puis repoussée de radius. Normales exactes, sans facettes.
        /// </summary>
        public static Mesh RoundedBox(Vector3 size, float radius)
        {
            Vector3 h = size * 0.5f;
            float r = Mathf.Clamp(radius, 0.0005f, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * 0.98f);
            Vector3 inner = h - Vector3.one * r;
            // Les petits arrondis n'ont pas besoin de beaucoup de pas : un seul suffit sous le centimètre.
            int steps = r < 0.01f ? 1 : r < 0.04f ? 2 : 4;

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            var coords = new float[3][];
            for (int axis = 0; axis < 3; axis++)
            {
                // Pas réguliers en angle sur l'arrondi (tan de 0 à 45°), une seule bande sur le plat.
                var list = new List<float>();
                for (int k = steps; k >= 0; k--) list.Add(-(inner[axis] + r * Mathf.Tan(Mathf.PI * 0.25f * k / steps)));
                for (int k = 0; k <= steps; k++) list.Add(inner[axis] + r * Mathf.Tan(Mathf.PI * 0.25f * k / steps));
                coords[axis] = list.ToArray();
            }

            for (int a = 0; a < 3; a++)
            {
                int u = (a + 1) % 3, w = (a + 2) % 3;
                for (int s = -1; s <= 1; s += 2)
                {
                    int start = vertices.Count, nu = coords[u].Length, nw = coords[w].Length;
                    for (int i = 0; i < nu; i++)
                    {
                        for (int j = 0; j < nw; j++)
                        {
                            var q = new Vector3();
                            q[a] = s * h[a];
                            q[u] = coords[u][i];
                            q[w] = coords[w][j];
                            var core = new Vector3(Mathf.Clamp(q.x, -inner.x, inner.x), Mathf.Clamp(q.y, -inner.y, inner.y),
                                Mathf.Clamp(q.z, -inner.z, inner.z));
                            Vector3 normal = (q - core).normalized;
                            vertices.Add(core + normal * r);
                            normals.Add(normal);
                        }
                    }
                    for (int i = 0; i < nu - 1; i++)
                    {
                        for (int j = 0; j < nw - 1; j++)
                        {
                            int p00 = start + i * nw + j, p10 = p00 + nw, p01 = p00 + 1, p11 = p10 + 1;
                            AddQuad(triangles, vertices, normals, p00, p10, p11, p01);
                        }
                    }
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            return HeightUV(mesh);
        }

        static void AddQuad(List<int> triangles, List<Vector3> v, List<Vector3> n, int a, int b, int c, int d)
        {
            AddTriangle(triangles, v, n, a, b, c);
            AddTriangle(triangles, v, n, a, c, d);
        }

        // Chaque triangle est tourné du côté de sa normale ; un triangle plat (bande vide) est ignoré.
        static void AddTriangle(List<int> triangles, List<Vector3> v, List<Vector3> n, int a, int b, int c)
        {
            Vector3 normal = Vector3.Cross(v[b] - v[a], v[c] - v[a]);
            if (normal.sqrMagnitude < 1e-14f) return;
            if (Vector3.Dot(normal, n[a] + n[b] + n[c]) < 0f)
            {
                int swap = b;
                b = c;
                c = swap;
            }
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        /// <summary>
        /// Coordonnée v = hauteur relative (0 en bas, 1 en haut) : le shader en tire le léger dégradé vertical
        /// des modèles ithappy, pour que les objets fabriqués par code aient la même lumière peinte.
        /// </summary>
        public static Mesh HeightUV(Mesh mesh)
        {
            mesh.RecalculateBounds();
            Bounds bounds = mesh.bounds;
            Vector3[] vertices = mesh.vertices;
            var uv = new Vector2[vertices.Length];
            float height = Mathf.Max(bounds.size.y, 0.0001f);
            for (int i = 0; i < vertices.Length; i++) uv[i] = new Vector2(0.5f, (vertices[i].y - bounds.min.y) / height);
            mesh.uv = uv;
            return mesh;
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
            mesh.SetTriangles(triangles, 0);
            return HeightUV(mesh);
        }

        /// <summary>
        /// Solide de révolution autour de l'axe vertical : profil donné en (rayon, hauteur), du bas vers le haut.
        /// Sert aux formes rondes et douces (abat-jour, coussins, corbeilles, pieds de lampe).
        /// </summary>
        public static Mesh Lathe(Vector2[] profile, int segments)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            int count = profile.Length, ring = segments + 1;

            for (int i = 0; i < count; i++)
            {
                Vector2 tangent = profile[Mathf.Min(i + 1, count - 1)] - profile[Mathf.Max(i - 1, 0)];
                Vector2 outward = new Vector2(tangent.y, -tangent.x).normalized;
                for (int s = 0; s <= segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / segments;
                    float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
                    vertices.Add(new Vector3(profile[i].x * cos, profile[i].y, profile[i].x * sin));
                    normals.Add(new Vector3(outward.x * cos, outward.y, outward.x * sin));
                }
            }
            for (int i = 0; i < count - 1; i++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int a = i * ring + s, b = a + 1, c = a + ring, d = c + 1;
                    triangles.AddRange(new[] { a, c, d, a, d, b });
                }
            }

            for (int cap = 0; cap < 2; cap++)
            {
                Vector2 end = cap == 0 ? profile[0] : profile[count - 1];
                if (end.x < 0.0005f) continue;
                Vector3 normal = cap == 0 ? Vector3.down : Vector3.up;
                int center = vertices.Count;
                vertices.Add(new Vector3(0f, end.y, 0f));
                normals.Add(normal);
                for (int s = 0; s < segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / segments;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * end.x, end.y, Mathf.Sin(angle) * end.x));
                    normals.Add(normal);
                }
                for (int s = 0; s < segments; s++)
                {
                    int a = center + 1 + s, b = center + 1 + (s + 1) % segments;
                    if (cap == 0) triangles.AddRange(new[] { center, a, b });
                    else triangles.AddRange(new[] { center, b, a });
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            return HeightUV(mesh);
        }

        /// <summary>
        /// Sphère de rayon 0,5 (diamètre 1, comme on l'attend d'une échelle) : la sphère intégrée de Unity
        /// fait ici le double, ce qui faussait toutes les tailles.
        /// </summary>
        public static Mesh Sphere(int segments = 32, int rings = 18)
        {
            var profile = new Vector2[rings + 1];
            for (int i = 0; i <= rings; i++)
            {
                float a = -Mathf.PI * 0.5f + Mathf.PI * i / rings;
                profile[i] = new Vector2(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f);
            }
            profile[0].x = 0f;
            profile[rings].x = 0f;
            return Lathe(profile, segments);
        }

        /// <summary>Anneau (tore) couché, centré à l'origine : anneaux d'empilement, bouées, ruban de médaille.</summary>
        public static Mesh Torus(float radius, float thickness, int segments = 32, int sides = 12)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                var around = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                for (int j = 0; j <= sides; j++)
                {
                    float b = j * Mathf.PI * 2f / sides;
                    Vector3 normal = around * Mathf.Cos(b) + Vector3.up * Mathf.Sin(b);
                    vertices.Add(around * radius + normal * thickness);
                    normals.Add(normal);
                }
            }
            int ring = sides + 1;
            for (int i = 0; i < segments; i++)
            {
                for (int j = 0; j < sides; j++)
                {
                    int p = i * ring + j, q = p + ring;
                    AddQuad(triangles, vertices, normals, p, p + 1, q + 1, q);
                }
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            return HeightUV(mesh);
        }

        /// <summary>Cylindre aux bords arrondis, posé sur sa base (y = 0).</summary>
        public static Mesh Puck(float radius, float height, float round, int segments)
        {
            round = Mathf.Min(round, Mathf.Min(radius, height * 0.5f) * 0.98f);
            const int steps = 5;
            var profile = new List<Vector2>();
            for (int i = 0; i <= steps; i++)
            {
                float angle = i * Mathf.PI * 0.5f / steps;
                profile.Add(new Vector2(radius - round + Mathf.Sin(angle) * round, round - Mathf.Cos(angle) * round));
            }
            for (int i = 0; i <= steps; i++)
            {
                float angle = i * Mathf.PI * 0.5f / steps;
                profile.Add(new Vector2(radius - round + Mathf.Cos(angle) * round, height - round + Mathf.Sin(angle) * round));
            }
            return Lathe(profile.ToArray(), segments);
        }

        /// <summary>Plaque aux coins arrondis vue de dessus (tapis), posée sur sa base (y = 0).</summary>
        public static Mesh RoundedSlab(float width, float depth, float height, float radius)
        {
            radius = Mathf.Min(radius, Mathf.Min(width, depth) * 0.49f);
            const int steps = 8;
            var outline = new List<Vector3>();
            var outward = new List<Vector3>();
            for (int corner = 0; corner < 4; corner++)
            {
                float cx = (corner == 0 || corner == 3 ? 1f : -1f) * (width * 0.5f - radius);
                float cz = (corner < 2 ? 1f : -1f) * (depth * 0.5f - radius);
                for (int i = 0; i <= steps; i++)
                {
                    float angle = (corner + (float)i / steps) * Mathf.PI * 0.5f;
                    var radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    outline.Add(new Vector3(cx, 0f, cz) + radial * radius);
                    outward.Add(radial);
                }
            }

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            int count = outline.Count;
            for (int i = 0; i < count; i++)
            {
                vertices.Add(outline[i]);
                normals.Add(outward[i]);
                vertices.Add(outline[i] + Vector3.up * height);
                normals.Add(outward[i]);
            }
            for (int i = 0; i < count; i++)
            {
                int a = i * 2, b = ((i + 1) % count) * 2;
                triangles.AddRange(new[] { a, a + 1, b + 1, a, b + 1, b });
            }

            int center = vertices.Count;
            vertices.Add(Vector3.up * height);
            normals.Add(Vector3.up);
            for (int i = 0; i < count; i++)
            {
                vertices.Add(outline[i] + Vector3.up * height);
                normals.Add(Vector3.up);
            }
            for (int i = 0; i < count; i++)
                triangles.AddRange(new[] { center, center + 1 + (i + 1) % count, center + 1 + i });

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            return HeightUV(mesh);
        }

        /// <summary>Brique de jeu à tenons arrondis, aux coins doux comme un jouet moulé ; origine au centre de sa base.</summary>
        public static Mesh Brick(int width, int depth, float height, float stud)
        {
            var parts = new List<CombineInstance>();
            Mesh body = RoundedBox(new Vector3(width * stud - 0.004f, height, depth * stud - 0.004f), 0.011f);
            parts.Add(new CombineInstance { mesh = body, transform = Matrix4x4.Translate(new Vector3(0f, height * 0.5f, 0f)) });

            Mesh knob = Puck(stud * 0.31f, stud * 0.24f, stud * 0.08f, 14);
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
            Object.DestroyImmediate(body);
            Object.DestroyImmediate(knob);
            return HeightUV(mesh);
        }

        /// <summary>
        /// Mur d'un seul tenant (x0..x1, 0..height, épaisseur centrée sur z = 0) percé d'une fenêtre :
        /// pas de joints entre panneaux, et le dégradé vertical court sur toute la hauteur du mur.
        /// </summary>
        public static Mesh WallWithWindow(float x0, float x1, float height, float thickness,
            float hx0, float hx1, float hy0, float hy1)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            float z = thickness * 0.5f;

            void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
            {
                int i = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
                for (int k = 0; k < 4; k++) normals.Add(normal);
                AddQuad(triangles, vertices, normals, i, i + 1, i + 2, i + 3);
            }

            // Faces avant et arrière : quatre bandes autour de la fenêtre.
            foreach (float side in new[] { -z, z })
            {
                Vector3 n = side < 0f ? Vector3.back : Vector3.forward;
                Face(new Vector3(x0, 0f, side), new Vector3(hx0, 0f, side), new Vector3(hx0, height, side), new Vector3(x0, height, side), n);
                Face(new Vector3(hx1, 0f, side), new Vector3(x1, 0f, side), new Vector3(x1, height, side), new Vector3(hx1, height, side), n);
                Face(new Vector3(hx0, 0f, side), new Vector3(hx1, 0f, side), new Vector3(hx1, hy0, side), new Vector3(hx0, hy0, side), n);
                Face(new Vector3(hx0, hy1, side), new Vector3(hx1, hy1, side), new Vector3(hx1, height, side), new Vector3(hx0, height, side), n);
            }

            // Tranches extérieures.
            Face(new Vector3(x0, 0f, -z), new Vector3(x0, 0f, z), new Vector3(x0, height, z), new Vector3(x0, height, -z), Vector3.left);
            Face(new Vector3(x1, 0f, -z), new Vector3(x1, 0f, z), new Vector3(x1, height, z), new Vector3(x1, height, -z), Vector3.right);
            Face(new Vector3(x0, height, -z), new Vector3(x1, height, -z), new Vector3(x1, height, z), new Vector3(x0, height, z), Vector3.up);
            Face(new Vector3(x0, 0f, -z), new Vector3(x1, 0f, -z), new Vector3(x1, 0f, z), new Vector3(x0, 0f, z), Vector3.down);

            // Tableau de la fenêtre (normales tournées vers l'ouverture).
            Face(new Vector3(hx0, hy0, -z), new Vector3(hx0, hy0, z), new Vector3(hx0, hy1, z), new Vector3(hx0, hy1, -z), Vector3.right);
            Face(new Vector3(hx1, hy0, -z), new Vector3(hx1, hy0, z), new Vector3(hx1, hy1, z), new Vector3(hx1, hy1, -z), Vector3.left);
            Face(new Vector3(hx0, hy0, -z), new Vector3(hx1, hy0, -z), new Vector3(hx1, hy0, z), new Vector3(hx0, hy0, z), Vector3.up);
            Face(new Vector3(hx0, hy1, -z), new Vector3(hx1, hy1, -z), new Vector3(hx1, hy1, z), new Vector3(hx0, hy1, z), Vector3.down);

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            return HeightUV(mesh);
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
