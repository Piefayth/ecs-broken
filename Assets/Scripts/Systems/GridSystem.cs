using Unity.Entities;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class GridSystem : ComponentSystem {
    public static MeshInstanceRenderer gridLook;
    public static EntityArchetype gridArchetype;

    bool initialized = false;

    protected override void OnUpdate() {
        if (!initialized) {
            Initialize();
            initialized = true;
        }
    }

    protected void Initialize() {
        EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

        gridLook = Bootstrap.GetLookFromPrototype("GridRenderPrototype");
        gridArchetype = entityManager.CreateArchetype(typeof(CustomGrid), typeof(Position), typeof(Scale));

        Entity grid = entityManager.CreateEntity(gridArchetype);
        gridLook.mesh = GenerateGridMesh(10, 10);

        entityManager.AddSharedComponentData(grid, gridLook);
        entityManager.SetComponentData(grid, new Position {
            Value = new float3(0.0f, 0.0f, 0.0f)
        });

        entityManager.SetComponentData(grid, new Scale {
            Value = new float3(1.0f, 1.0f, 1.0f)
        });

        entityManager.SetComponentData(grid, new CustomGrid {
            xSize = 10,
            ySize = 10
        });

        float3[] origins = new float3[gridLook.mesh.vertices.Length];
        for (int i = 0; i < origins.Length; i++) {
            Entity sphereRequest = entityManager.CreateEntity(typeof(SphereRequest));
            entityManager.SetComponentData(sphereRequest, new SphereRequest {
                origin = new float3(gridLook.mesh.vertices[i].x, gridLook.mesh.vertices[i].y, gridLook.mesh.vertices[i].z)
            });
        }
    }

    private static Mesh GenerateGridMesh(int xSize, int ySize) {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        int i = 0;
        for (int y = 0; y <= ySize; y++) {
            for (int x = 0; x <= xSize; x++) {
                i = (y * xSize) + x + y;
                vertices[i] = new Vector3(x, y);
                uvs[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.tangents = tangents;

        int[] triangles = new int[xSize * ySize * 6];

        int ti, vi;
        for (int y = 0; y < ySize; y++) {
            for (int x = 0; x < xSize; x++) {
                vi = (y * xSize) + x + y;
                ti = 6 * ((y * xSize) + x);

                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}
