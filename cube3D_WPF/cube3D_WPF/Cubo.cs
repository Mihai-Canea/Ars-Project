using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Controls;

namespace cube3D_WPF
{
    public enum Faccie
    {
        F, // front   -->    davanti
        R, // right   -->    destra
        B, // back    -->    dietro
        L, // left    -->    sinistra
        U, // up      -->    sopra
        D, // down    -->    sotto
        None
    }


    /// <summary>
    /// Create a cube
    /// 
    /// In order to display the cube to a <see cref="ViewPort3D"/>:
    /// <code>
    /// Cube c = new Cube(new Point3D(0, 0, 0), 1, new Dictionary&lt;CubeFace, Material&gt;() {
    ///     {CubeFace.F, new DiffuseMaterial(new SolidColorBrush(Colors.White))},
    ///     {CubeFace.R, new DiffuseMaterial(new SolidColorBrush(Colors.Blue))},
    ///     {CubeFace.U, new DiffuseMaterial(new SolidColorBrush(Colors.Yellow))},
    /// });
    /// 
    /// ModelVisual3D m = new ModelVisual3D();
    /// m.Content = c.group;
    /// 
    /// this.mainViewport.Children.Add(m);
    /// </code>
    /// </summary>
    class Cubo : ModelVisual3D
    {
        private Point3D origin;
        private double edge_len;

        private Material defaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
        private Dictionary<Faccie, Material> faces;
        public HashSet<Move> possibleMoves = new HashSet<Move>();
        public Transform3DGroup rotations = new Transform3DGroup();

        /// <summary>
        /// Cube constructor
        /// </summary>
        /// <param name="o">The origin of the cube, this will always be the far-lower-left corner</param>
        /// <param name="len">The length of the edge</param>
        /// <param name="f">This parameter allows the use of different materials on the cube's faces</param>
        /// <param name="defaultMaterial">The material to be applied on the faces 
        /// that are not included in the previous parameter.
        /// This defaults to a solid black diffuse material
        /// </param>
        public Cubo(Point3D o, double len, Dictionary<Faccie, Material> f, HashSet<Move> possibleMoves, Material defaultMaterial = null)
        {
            this.origin = o;
            this.edge_len = len;
            this.faces = f;
            this.possibleMoves = possibleMoves;

            if (defaultMaterial != null)
            {
                this.defaultMaterial = defaultMaterial;
            }

            this.Transform = this.rotations;

            creaCubo();
        }

        protected Cubo() { }

        /// <summary>
        /// Creates the cube by creating it's 6 faces
        /// If the class was instantiated with a valid <see cref="f"/> dictionary 
        /// then each face will get the corespondent Material, 
        /// otherwise <see cref="defaultMaterial"/> will be used
        /// </summary>
        protected virtual void creaCubo()
        {
            Material material;

            foreach (var face in Enum.GetValues(typeof(Faccie)).Cast<Faccie>())
            {
                if (faces == null || !faces.TryGetValue(face, out material))
                {
                    material = defaultMaterial;
                }

                createFace(face, material);
            }
        }

        /// <summary>
        /// Create a face of the cube
        /// </summary>
        /// <param name="f">The face that needs to be created</param>
        /// <param name="m">Materal to be applied to the face</param>
        private void createFace(Faccie f, Material m)
        {
            Point3D pos0 = new Point3D();
            Point3D pos1 = new Point3D();
            Point3D pos2 = new Point3D();
            Point3D pos3 = new Point3D();

            switch (f)
            {
                case Faccie.F:
                    /**
                     *  /--------/
                     * 0-------3 |
                     * |       | |
                     * |       | /
                     * 1-------2/
                     */
                    pos0.X = origin.X;
                    pos0.Y = origin.Y + edge_len;
                    pos0.Z = origin.Z + edge_len;
                     
                    pos1.X = origin.X;
                    pos1.Y = origin.Y;
                    pos1.Z = origin.Z + edge_len;
                     
                    pos2.X = origin.X + edge_len;
                    pos2.Y = origin.Y;
                    pos2.Z = origin.Z + edge_len;
                     
                    pos3.X = origin.X + edge_len;
                    pos3.Y = origin.Y + edge_len;
                    pos3.Z = origin.Z + edge_len;
                    break;
                case Faccie.R:
                    /**
                     *  /--------3
                     * /-------0 |
                     * |       | |
                     * |       | 2
                     * |-------1/
                     */
                    pos0.X = origin.X + edge_len;
                    pos0.Y = origin.Y + edge_len;
                    pos0.Z = origin.Z + edge_len;
                     
                    pos1.X = origin.X + edge_len;
                    pos1.Y = origin.Y;
                    pos1.Z = origin.Z + edge_len;
                     
                    pos2.X = origin.X + edge_len;
                    pos2.Y = origin.Y;
                    pos2.Z = origin.Z;
                     
                    pos3.X = origin.X + edge_len;
                    pos3.Y = origin.Y + edge_len;
                    pos3.Z = origin.Z;
                    break;
                case Faccie.B:
                    /**
                     *  3--------0
                     * /-------/ |
                     * | |     | |
                     * | 2 ----|-1
                     * |-------|/
                     */
                    pos0.X = origin.X + edge_len;
                    pos0.Y = origin.Y + edge_len;
                    pos0.Z = origin.Z;
                     
                    pos1.X = origin.X + edge_len;
                    pos1.Y = origin.Y;
                    pos1.Z = origin.Z;

                    pos2 = origin;

                    pos3.X = origin.X;
                    pos3.Y = origin.Y + edge_len;
                    pos3.Z = origin.Z;
                    break;
                case Faccie.L:
                    /**
                     *  0--------/
                     * 3-------/ |
                     * | |     | |
                     * | 1 ----|-/
                     * 2-------|/
                     */
                    pos0.X = origin.X;
                    pos0.Y = origin.Y + edge_len;
                    pos0.Z = origin.Z;
                     
                    pos1 = origin;
                     
                    pos2.X = origin.X;
                    pos2.Y = origin.Y;
                    pos2.Z = origin.Z + edge_len;
                     
                    pos3.X = origin.X;
                    pos3.Y = origin.Y + edge_len;
                    pos3.Z = origin.Z + edge_len;
                    break;
                case Faccie.U:
                    /**
                     *  0--------3
                     * 1-------2 |
                     * |       | |
                     * |       | |
                     * |-------|/
                     */
                    pos0.X = origin.X;
                    pos0.Y = origin.Y + edge_len;
                    pos0.Z = origin.Z;
                     
                    pos1.X = origin.X;
                    pos1.Y = origin.Y + edge_len;
                    pos1.Z = origin.Z + edge_len;
                     
                    pos2.X = origin.X + edge_len;
                    pos2.Y = origin.Y + edge_len;
                    pos2.Z = origin.Z + edge_len;
                     
                    pos3.X = origin.X + edge_len;
                    pos3.Y = origin.Y + edge_len;
                    pos3.Z = origin.Z;
                    break;
                case Faccie.D:
                    /**
                     *  /--------/
                     * /-------/ |
                     * | |     | |
                     * | 0 ----|-1
                     * 3-------|2
                     */
                    pos0 = origin;
                     
                    pos1.X = origin.X + edge_len;
                    pos1.Y = origin.Y;
                    pos1.Z = origin.Z;
                     
                    pos2.X = origin.X + edge_len;
                    pos2.Y = origin.Y;
                    pos2.Z = origin.Z + edge_len;
                     
                    pos3.X = origin.X;
                    pos3.Y = origin.Y;
                    pos3.Z = origin.Z + edge_len;
                    break;
            }

            ModelVisual3D r1 = new ModelVisual3D();
            ModelVisual3D r2 = new ModelVisual3D();

            Border aa = new Border();

            r1.Content = Helpers.createTriangleModel(pos0, pos1, pos2, m);
            r2.Content = Helpers.createTriangleModel(pos0, pos2, pos3, m);


            this.Children.Add(r1);
            this.Children.Add(r2);
        }
    }
}
