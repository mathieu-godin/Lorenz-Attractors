using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Lorenz_Attractors
{
    public class CylindreTextur� : PrimitiveDeBaseAnim�e//, ICollisionable
    {
        //Initialement g�r�es par le constructeur
        //readonly Vector2 Delta;
        readonly int NbColonnes;
        readonly int NbLignes;

        protected string NomTexture { get; set; }

        readonly Vector3 Origine;

        //Initialement g�r�es par des fonctions appell�es par base.Initialize()
        Vector3[,] PtsSommets { get; set; }
        Vector2[,] PtsTexture { get; set; }
        VertexPositionTexture[] Sommets { get; set; }
        BasicEffect EffetDeBase { get; set; }

        int NbTrianglesParStrip { get; set; }

        //Initialement g�r�es par LoadContent()
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        Texture2D TextureCylindre { get; set; }

        //public bool EstEnCollision(object autreObjet)
        //{
        //    return Sph�reDeCollision.Intersects((autreObjet as CubeTextur�).Sph�reDeCollision);
        //}

        //public BoundingSphere Sph�reDeCollision { get { return new BoundingSphere(Position, Rayon); } }

        Vector3 Extr�mit�1 { get; set; }
        Vector3 Extr�mit�2 { get; set; }



        public CylindreTextur�(Game jeu, float homoth�tieInitiale, Vector3 rotationInitiale,
                              Vector3 positionInitiale, Vector2 �tendue, Vector2 charpente,
                              string nomTexture, float intervalleMAJ, Vector3 extr�mit�1, Vector3 extr�mit�2)
            : base(jeu, homoth�tieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            //Delta = �tendue / charpente;
            NbColonnes = (int)charpente.X;
            NbLignes = (int)charpente.Y;
            NomTexture = nomTexture;

            //Origine = new Vector3(0,0,0);
            Origine = new Vector3(-�tendue.X / 2, -�tendue.Y / 2, 0);

            Extr�mit�1 = extr�mit�1;
            Extr�mit�2 = extr�mit�2;

        }

        public override void Initialize()
        {
            NbTrianglesParStrip = NbColonnes * 2;
            NbSommets = (NbTrianglesParStrip + 2) * NbLignes;

            AllouerTableaux();
            base.Initialize();
            InitialiserParam�tresEffetDeBase();
        }

        void AllouerTableaux()
        {
            PtsSommets = new Vector3[NbColonnes + 1, NbLignes + 1];
            PtsTexture = new Vector2[NbColonnes + 1, NbLignes + 1];
            Sommets = new VertexPositionTexture[NbSommets];
        }

        protected void InitialiserParam�tresEffetDeBase()
        {
            TextureCylindre = GestionnaireDeTextures.Find(NomTexture);
            EffetDeBase = new BasicEffect(GraphicsDevice);
            EffetDeBase.TextureEnabled = true;
            EffetDeBase.Texture = TextureCylindre;
        }

        protected override void InitialiserSommets()
        {
            AffecterPtsSommets();
            AffecterPtsTexture();
            AffecterSommets();
        }

        void AffecterPtsSommets()
        {
            for (int i = 0; i < PtsSommets.GetLength(0); ++i)
            {
                for (int j = 0; j < PtsSommets.GetLength(1); ++j)
                {
                    PtsSommets[i, j] = new Vector3(Origine.X - (i/NbColonnes * (Extr�mit�2 - Extr�mit�1).Length()* (Vector3.Normalize(Extr�mit�1 - Extr�mit�2).X)) + Extr�mit�1.X,
                                                   Origine.Y - (i / NbColonnes * (Extr�mit�2 - Extr�mit�1).Length() * (Vector3.Normalize(Extr�mit�1 - Extr�mit�2).Y)) + (float)Math.Cos(j * 2 * Math.PI / NbLignes) + Extr�mit�1.Y,
                                                   Origine.Z - (i / NbColonnes * (Extr�mit�2 - Extr�mit�1).Length() * (Vector3.Normalize(Extr�mit�1 - Extr�mit�2).Z))+(float)Math.Sin(j * 2 * Math.PI / NbLignes) + Extr�mit�1.Z);
                }
            }
        }

        void AffecterPtsTexture()
        {
            for (int i = 0; i < PtsTexture.GetLength(0); ++i)
            {
                for (int j = 0; j < PtsTexture.GetLength(1); ++j)
                {
                    PtsTexture[i, j] = new Vector2(i / (float)NbColonnes, -j / (float)NbLignes);
                }
            }
        }

        protected void AffecterSommets()
        {
            int NoSommet = -1;
            for (int j = 0; j < NbLignes; ++j)
            {
                for (int i = 0; i < NbColonnes + 1; ++i)
                {
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j], PtsTexture[i, j]);
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j + 1], PtsTexture[i, j + 1]);
                }
            }
        }

        protected override void LoadContent()
        {
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = Cam�raJeu.Vue;
            EffetDeBase.Projection = Cam�raJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                passeEffet.Apply();
                for (int i = 0; i < NbLignes; ++i)
                {
                    DessinerTriangleStrip(i);
                }
            }
        }

        void DessinerTriangleStrip(int noStrip)
        {
            int vertexOffset = (noStrip * NbSommets) / NbLignes;
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Sommets, vertexOffset, NbTrianglesParStrip);
        }

    }
}
