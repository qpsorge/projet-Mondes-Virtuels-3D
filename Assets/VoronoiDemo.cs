using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using UnityEngine.AI;

public class VoronoiDemo : MonoBehaviour
{
	public GameObject immeuble;
	public GameObject house;
	public GameObject road;
    public Material land;
	public GameObject plane;
	public GameObject environment;
	public GameObject agent;

	public const int NB_AGENT_PER_HOUSE = 1;
	public const float NBHOUSE_ROAD = 2f;
	public const int NPOINTS = 10;
    public const int WIDTH_VORONOI  = 1000;
    public const int HEIGHT_VORONOI = 1000;
	public float freqx = 0.02f, freqy = 0.018f, offsetx = 0.43f, offsety = 0.22f;
	private int PLANE_SIZE = 10;
	private int PLANE_CELL_NB; // = 10 * PLANE_SIZE;
	static public float[,] map;


	private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private Texture2D tx;
	private List<GameObject> ListImmeubles;
	private List<GameObject> ListHouses;

	private float [,] createMap() 
    {
        float [,] map = new float[WIDTH_VORONOI, HEIGHT_VORONOI];
        for (int i = 0; i < WIDTH_VORONOI; i++)
            for (int j = 0; j < HEIGHT_VORONOI; j++)
                map[i, j] = 2f * Mathf.PerlinNoise(freqx * i + offsetx, freqy * j + offsety);
        return map;
    }

	void Awake ()
	{
		ListImmeubles = new List<GameObject>();
		ListHouses = new List<GameObject>();

		PLANE_CELL_NB = 10 * PLANE_SIZE;
		map = createMap();
        Color[] pixels = createPixelMap(map);

        /* Create random points points */
		
		m_points = new List<Vector2> ();
		List<uint> colors = new List<uint> ();
		for (int i = 0; i < NPOINTS; i++) {
			colors.Add ((uint)0);

			int x_coord = Random.Range(0, WIDTH_VORONOI - 1);
			int y_coord = Random.Range(0, HEIGHT_VORONOI - 1);
			int nb_try = 0;
			while(map[x_coord,y_coord]< 0.7 && nb_try<50)
            {
				x_coord = Random.Range(0, WIDTH_VORONOI - 1);
				y_coord = Random.Range(0, HEIGHT_VORONOI - 1);
				nb_try++;
			}
			Vector2 vec = new Vector2(x_coord, y_coord);
			m_points.Add (vec);
		}


		/* Generate immeubles where points are */
		foreach(Vector2 point in m_points)
        {
			GameObject immeuble_go = Instantiate(immeuble, new Vector3(
						(point.x/ WIDTH_VORONOI-0.5f) * PLANE_CELL_NB + immeuble.transform.position.x,
								  immeuble.transform.position.y,
						(point.y / HEIGHT_VORONOI-0.5f) * PLANE_CELL_NB + immeuble.transform.position.z
						),
						immeuble.transform.rotation);
			ListImmeubles.Add(immeuble_go);
		}

		/* Generate Graphs */
		Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, WIDTH_VORONOI, HEIGHT_VORONOI));
		m_edges = v.VoronoiDiagram ();
		m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation ();

		Debug.Log("Centroides " + m_points.Count + " Edges " + m_edges.Count);

		/* Shows Voronoi diagram */
		Color color = Color.blue;
		float left_real_x;
		float left_real_y;
		float right_real_x;
		float right_real_y;
		for (int i = 0; i < m_edges.Count; i++) {
			LineSegment seg = m_edges [i];				
			Vector2 left = (Vector2)seg.p0;
			Vector2 right = (Vector2)seg.p1;

			left_real_x = (left.x / HEIGHT_VORONOI - 0.5F) * PLANE_CELL_NB;
			left_real_y = (left.y / WIDTH_VORONOI - 0.5F) * PLANE_CELL_NB;
			right_real_x = (right.x / HEIGHT_VORONOI - 0.5F) * PLANE_CELL_NB;
			right_real_y = (right.y / WIDTH_VORONOI - 0.5F) * PLANE_CELL_NB;
			Vector2 segment = (right - left) / WIDTH_VORONOI * PLANE_SIZE;
			Debug.Log("Magnitude segment : " + segment.magnitude);
			float angle = Vector2.SignedAngle(Vector2.right, right-left);



			if(segment.magnitude>1.5f) //Spawn houses around road if and only if road is long enough
            {
				for (float j = 1; j < NBHOUSE_ROAD; j++)
				{
					//Debug.Log("x" + left_real_y + "z" + left_real_x);
					//Debug.Log("End x" + right_real_y + "End z" + right_real_x);
					//Debug.Log("house pos " + left_real_y + (j / NBHOUSE_ROAD) * right_real_y);

					/*
						(1 - (j / NBHOUSE_ROAD)) * left_real_y + (j / NBHOUSE_ROAD) * ((right - left).y / HEIGHT_VORONOI - 0.5F) * PLANE_CELL_NB, 
						immeuble.transform.position.y, 
						(1 - (j / NBHOUSE_ROAD)) * left_real_x + (j / NBHOUSE_ROAD) * ((right - left).x / HEIGHT_VORONOI - 0.5F) * PLANE_CELL_NB
						*/

					GameObject mush_house = Instantiate(house, new Vector3(
						left_real_y + (j / NBHOUSE_ROAD) * (right_real_y - left_real_y) + house.transform.position.x,
																						  house.transform.position.y,
						left_real_x + (j / NBHOUSE_ROAD) * (right_real_x - left_real_x) + house.transform.position.z
						),
						house.transform.rotation);

					ListHouses.Add(mush_house);

					// Add house light to light controller
					LightController.house_light.Add(mush_house.GetComponentInChildren<Light>());
				}
			}
			
			GameObject go = Instantiate(road, new Vector3(left_real_y, 0, left_real_x), Quaternion.Euler(new Vector3(0,angle+90,0))); //plane 10x10 starts at -5
			go.transform.localScale = new Vector3(segment.magnitude, 1, 1);
			go.transform.parent = environment.transform;


			DrawLine (pixels,left, right,color);
		}

		//List<Vector2> sitecoords = v.SiteCoords();
		for (int i = 0; i < m_points.Count; i++) //m_points
        {
			//Vector2 pos = (sitecoords[i]/WIDTH_VORONOI*10-new Vector2(5,5))* PLANE_SIZE;
			//Instantiate(immeuble, new Vector3(pos.x, 0.1f,pos.y), Quaternion.identity);
			
			//GameObject go = Instantiate(immeuble, new Vector3((m_points[i].y / WIDTH_VORONOI - 0.5F)*PLANE_CELL_NB, immeuble.transform.position.y, (m_points[i].x / HEIGHT_VORONOI -0.5F)*PLANE_CELL_NB), immeuble.transform.rotation);
		}

		

		/* Shows Delaunay triangulation */
		/*
 		color = Color.red;
		if (m_delaunayTriangulation != null) {
			for (int i = 0; i < m_delaunayTriangulation.Count; i++) {
					LineSegment seg = m_delaunayTriangulation [i];				
					Vector2 left = (Vector2)seg.p0;
					Vector2 right = (Vector2)seg.p1;
					DrawLine (pixels,left, right,color);
			}
		}*/

		/* Shows spanning tree */
		/*
		color = Color.black;
		if (m_spanningTree != null) {
			for (int i = 0; i< m_spanningTree.Count; i++) {
				LineSegment seg = m_spanningTree [i];				
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				DrawLine (pixels,left, right,color);
			}
		}*/

		/* Apply pixels to texture */
		tx = new Texture2D(WIDTH_VORONOI, HEIGHT_VORONOI);
        land.SetTexture ("_MainTex", tx);
		tx.SetPixels (pixels);
		tx.Apply (); //texture envoyée vers la carte graphique pour devenir visible

		environment.GetComponent<NavMeshSurface>().BuildNavMesh();

		// Spawn agent with a house and an immeuble to work
		for (int i = 0; i < ListHouses.Count; i++)
        {
			GameObject mush_house = ListHouses[i];
			for (int k = 0; k < NB_AGENT_PER_HOUSE; k++) 
            {
				GameObject agent_go = Instantiate(agent, new Vector3(
				1+Random.Range(0, 5f) + mush_house.transform.position.x,
																 0.08f,
				1+Random.Range(0, 5f) + mush_house.transform.position.z)
				, mush_house.transform.rotation);
				agent_go.GetComponent<move>().home = mush_house;
				agent_go.GetComponent<move>().immeuble = ListImmeubles[Random.Range(0, ListImmeubles.Count - 1)];
				LightController.agents.Add(agent_go);
			}
		}
		LightController.nb_agent_total = LightController.agents.Count;
		
	}



    /* Functions to create and draw on a pixel array */
    private Color[] createPixelMap(float[,] map)
    {
        Color[] pixels = new Color[WIDTH_VORONOI * HEIGHT_VORONOI];
        for (int i = 0; i < WIDTH_VORONOI; i++)
            for (int j = 0; j < HEIGHT_VORONOI; j++)
            {
                pixels[i * HEIGHT_VORONOI + j] = Color.Lerp(Color.black, Color.white, map[i, j]);
            }
        return pixels;
    }
    private void DrawPoint (Color [] pixels, Vector2 p, Color c) {
		if (p.x<WIDTH_VORONOI&&p.x>=0&&p.y<HEIGHT_VORONOI&&p.y>=0) 
		    pixels[(int)p.x*HEIGHT_VORONOI+(int)p.y]=c;
	}
	// Bresenham line algorithm
	private void DrawLine(Color [] pixels, Vector2 p0, Vector2 p1, Color c) {
		int x0 = (int)p0.x;
		int y0 = (int)p0.y;
		int x1 = (int)p1.x;
		int y1 = (int)p1.y;

		int dx = Mathf.Abs(x1-x0);
		int dy = Mathf.Abs(y1-y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx-dy;
		while (true) {
            if (x0>=0&&x0<WIDTH_VORONOI&&y0>=0&&y0<HEIGHT_VORONOI)
    			pixels[x0*HEIGHT_VORONOI+y0]=c;

			if (x0 == x1 && y0 == y1) break;
			int e2 = 2*err;
			if (e2 > -dy) {
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx) {
				err += dx;
				y0 += sy;
			}
		}
	}
}