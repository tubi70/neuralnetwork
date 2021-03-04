using System;
using System.Drawing;

namespace AI_Life
{
    //class for individual ant
    public class Ants
    {
        //---------------------------------------------------------//
        //              Special Thanks to Jan Krumsiek             //
        //---------------------------------------------------------//


        // a lot part of this and other code is self-explanatory;//
        // just interpret them as they are in simple english     //
        //=========================================================//
        public double x, y;				//ant location
        public double angle;
        double dir;			//its direction
        PointF myFood;
        public static int FOV = 100;    //field of view
        public bool invisible;// when agent get food, they become invisible
        public double antMaxSpeed = 8;
        // constants for org drawing
        public static int AntTopX = 0;
        public static int AntTopY = -14;
        public static int AntBottomX = 0;
        public static int AntBottomY = 14;
        public static int AntLeftX = -6;
        public static int AntLeftY = 4;
        public static int AntRightX = 6;
        public static int AntRightY = 4;
        public string agentType;
        //=========================================================//
        private Size clientSize;
        int indMyFood;			//its food
        int foodCollected = 0;
        int antNumber = 0;
        //=========================================================//
        public Network net;		//private network
        Cosmos myWorld;		//the world
        private PointF startPosition, finishPosition;   //used to find the distance traveled. so that we can make a choice
        //used in land mines case
        int[] myFoodCollected;

        public Ants(Size ClientSize, Cosmos MyWorld, Array weights, int AntNumber)
        {
            myFoodCollected = new int[MyWorld.getNumGoals()];

            invisible = false;
            clientSize = ClientSize;
            myWorld = MyWorld;
            x = Cosmos.randomR.Next(clientSize.Width);
            y = Cosmos.randomR.Next(clientSize.Height);
            angle = 0; // kola 2/26 add angle for obstacle avoidance
            dir = (float)(Cosmos.randomR.NextDouble());
            net = new Network(4, 1, 6, 2);  //initialize network and weights
            if (weights != null)
                this.net.Weights = weights;

            antNumber = AntNumber;
            startPosition = new PointF((float)x, (float)y);
            finishPosition = startPosition;
        }

        /*public void UpdatePosition(Polygon[] polygons, int numFood)
        {
            double foodDist = 0;
            double leftTrack;
            double rightTrack;
            double curSpeed;

          

            myFood = myWorld.FindClosestFood(x, y, ref foodDist, ref indMyFood, myFoodCollected);
            double[] input = new double[4];
            input[0] = Math.Cos(dir);   //could be sin also; doesn't matter aslong as its between 0 and 1
            input[1] = Math.Cos(dir);
            input[2] = (myFood.X - x) / foodDist;   //let the ant know about nearest food
            input[3] = (myFood.Y - y) / foodDist;   //same
            double[] output = net.FeedData(input);  //feed input into network and get output
            leftTrack = output[0];
            rightTrack = output[1];
            dir += (rightTrack - leftTrack) * (Cosmos.maxForce / 100);   //find the direction
            curSpeed = (rightTrack + leftTrack) / 2;
            x += Math.Sin(dir) * antMaxSpeed * curSpeed / 10;       //max speed is just a twaking parameter; don't get confused by it
            y -= Math.Cos(dir) * antMaxSpeed * curSpeed / 10;       //try varying it in simulation

            Vector2 velocity = new Vector2((float)x, (float)y);

            int tx, ty, lx, ly, rx, ry;
            tx = (int)(Ants.AntTopX * Math.Cos(dir) - Ants.AntTopY * Math.Sin(dir) + x);
            ty = (int)(Ants.AntTopY * Math.Cos(dir) + Ants.AntTopX * Math.Sin(dir) + y);
            lx = (int)(Ants.AntLeftX * Math.Cos(dir) - Ants.AntLeftY * Math.Sin(dir) + x);
            ly = (int)(Ants.AntLeftY * Math.Cos(dir) + Ants.AntLeftX * Math.Sin(dir) + y);
            rx = (int)(Ants.AntRightX * Math.Cos(dir) - Ants.AntRightY * Math.Sin(dir) + x);
            ry = (int)(Ants.AntRightY * Math.Cos(dir) + Ants.AntRightX * Math.Sin(dir) + y);
            //g.FillPolygon(Cosmos.whiteBrush, new Point[] { new Point(tx, ty), new Point(lx, ly), new Point(rx, ry) });

            Polygon agent = new Polygon();
            agent.Points.Add(new Vector2(tx, ty));
            agent.Points.Add(new Vector2(lx, ly));
            agent.Points.Add(new Vector2(rx, ry));
            agent.BuildEdges();
            Vector2 playerTranslation = velocity;
           
            for (int m = 0; m < polygons.Length; m++)
            {
                PolygonCollisionResult r = PolygonCollision(agent, polygons[m], velocity);

                if (r.WillIntersect)
                {
                    playerTranslation = velocity + r.MinimumTranslationVector;
                    x = x + playerTranslation.X/2;
                    y = y + playerTranslation.Y/2;
                    
                    // y = y + playerTranslation.Y;
                    break;
                }
            }

            
            Vector2 finish = new Vector2();
            
            if (agentType != null && agentType.Equals("calm"))
            {
                finish = new Vector2(finishPosition.X, finishPosition.Y);
                Vector2 target = new Vector2(myFood.X,myFood.Y);
                Vector2 current = new Vector2((float)x,(float)y);
                Vector2 steerForce = SteeringBehaviours.Seek(ref target,ref current , ref velocity, 10);
                steerForce = Vector2.Truncate(steerForce, 10);
                Random random = new Random();            
                Vector2 acceleration = steerForce / (random.Next(20, 80));
                velocity = Vector2.Truncate(velocity + acceleration,10);
                finish = Vector2.Add(velocity, finish);
                finishPosition = new PointF((float)finish.X, (float)finish.Y);
            }
            else
            finishPosition = new PointF((float)x + finishPosition.X, (float)y + finishPosition.Y);
           
             

            if (x < 0)
                x = clientSize.Width;
            if (x > clientSize.Width)
                x = 0;
            if (y < 0)
                y = clientSize.Height;
            if (y > clientSize.Height)
                y = 0;

          

            if ((myFood.X >= (x - Cosmos.foodTolerance)) &&
                (myFood.X <= (x + Cosmos.foodTolerance)) &&
                (myFood.Y >= (y - Cosmos.foodTolerance)) &&
                (myFood.Y <= (y + Cosmos.foodTolerance)))
            {
                if (myFoodCollected[indMyFood] != 1)
                {
                    foodCollected++;
                    myFoodCollected[indMyFood] = 1;
                }

                // agent disappears when they have reached the total number of goals
                if(foodCollected == numFood)
                    invisible = true;
                myWorld.NewFood(indMyFood);
            }
        } */



        public void UpdatePosition(Polygon[] polygons, int numFood)
        {
            double foodDist = 0;
            double leftTrack;
            double rightTrack;
            double curSpeed;

            myFood = myWorld.FindClosestFood(x, y, ref foodDist, ref indMyFood, myFoodCollected);
            
            

            double[] input = new double[4];
            input[0] = Math.Cos(dir);   //could be sin also; doesn't matter aslong as its between 0 and 1
            input[1] = Math.Cos(dir);
            input[2] = (myFood.X - x) / foodDist;   //let the ant know about nearest food
            input[3] = (myFood.Y - y) / foodDist;   //same
            double[] output = net.FeedData(input);  //feed input into network and get output
            leftTrack = output[0];
            rightTrack = output[1];
            double olddir = dir;
            dir += (rightTrack - leftTrack) * (Cosmos.maxForce / 100);   //find the direction
            curSpeed = (rightTrack + leftTrack) / 2;
            x += Math.Sin(dir) * antMaxSpeed * curSpeed / 10;       //max speed is just a twaking parameter; don't get confused by it
            y -= Math.Cos(dir) * antMaxSpeed * curSpeed / 10;       //try varying it in simulation
      //      finishPosition = new PointF((float)x + finishPosition.X, (float)y + finishPosition.Y);


            if (((x - 15) <= 5) || ((x + 15) >= 1090))
            {
                if ((x - 15) <= 5)
                    x += 15;
                else
                    x -= 15;
            }


            if (((y - 15) <= 5) || ((y + 15) >= 667))
            {
                if ((y - 15) <= 5)
                    y += 15;
                else
                    y -= 15;
            }  
            
            Vector2 targetPosition = new Vector2(myFood);
            Vector2 currentPosition = new Vector2((float)x, (float)y);
            Vector2 desired_V = Vector2.Normalize(Vector2.Subtract(targetPosition, currentPosition)) * (int)antMaxSpeed;
            Vector2 velocity = new Vector2(1, 1);
            Vector2 steerForce = Vector2.Subtract(desired_V, velocity);
            steerForce = Vector2.Truncate(steerForce, (int)Cosmos.maxForce);
            float mass = 21;
            Vector2 acceleration = steerForce / mass;
            velocity = Vector2.Truncate(velocity + acceleration, (int)antMaxSpeed);
            currentPosition = Vector2.Add(velocity, currentPosition);
            finishPosition = new PointF((float)currentPosition.X + finishPosition.X, (float)currentPosition.Y + finishPosition.Y);
            x = (float)currentPosition.X;
            y = (float)currentPosition.Y;  

            int tx, ty, lx, ly, rx, ry;
            tx = (int)(Ants.AntTopX * Math.Cos(dir) - Ants.AntTopY * Math.Sin(dir) + x);
            ty = (int)(Ants.AntTopY * Math.Cos(dir) + Ants.AntTopX * Math.Sin(dir) + y);
            lx = (int)(Ants.AntLeftX * Math.Cos(dir) - Ants.AntLeftY * Math.Sin(dir) + x);
            ly = (int)(Ants.AntLeftY * Math.Cos(dir) + Ants.AntLeftX * Math.Sin(dir) + y);
            rx = (int)(Ants.AntRightX * Math.Cos(dir) - Ants.AntRightY * Math.Sin(dir) + x);
            ry = (int)(Ants.AntRightY * Math.Cos(dir) + Ants.AntRightX * Math.Sin(dir) + y);
            Polygon agent = new Polygon();
            agent.Points.Add(new Vector2(tx, ty));
            agent.Points.Add(new Vector2(lx, ly));
            agent.Points.Add(new Vector2(rx, ry));
            agent.BuildEdges();
            Vector2 playerTranslation = velocity;

            for (int m = 0; m < polygons.Length; m++)
            {
                if ((rx + 270) >= polygons[m].Points[3].X)
                {
                    PolygonCollisionResult r = PolygonCollision(agent, polygons[m], velocity);

                    if (r.WillIntersect)
                    {
                        playerTranslation = velocity + r.MinimumTranslationVector;
                        x = x + playerTranslation.X / 2;
                        y = y + playerTranslation.Y / 2;

                        dir = olddir;
                        // y = y + playerTranslation.Y;
                        break;
                    }
                }
            } 
              /*for (int m = 0; m < polygons.Length; m++)
             {
                 RectangleF rf = new RectangleF(polygons[m].Points[0].X,polygons[m].Points[0].Y, Math.Abs(polygons[m].Points[1].X - polygons[m].Points[0].X),Math.Abs(polygons[m].Points[2].Y - polygons[m].Points[1].Y));
             
                 if(IsIntersected(new PointF((float)x + finishPosition.X, (float)y + finishPosition.Y),(float)6, rf))
                 {
                   // playerTranslation = velocity + r.MinimumTranslationVector;
                    //x = x + playerTranslation.X / 2;
                    //y = y + playerTranslation.Y / 2;
                     x = x + 50;
                     y = y + 50;
                    
                    // y = y + playerTranslation.Y;
                    break;
                }
            } */
           

            //finishPosition = new PointF((float)x + finishPosition.X, (float)y + finishPosition.Y);
            if (x < 0)
                x = clientSize.Width;
            if (x > clientSize.Width)
                x = 0;
            if (y < 0)
                y = clientSize.Height;
            if (y > clientSize.Height)
                y = 0;

          
            if ((myFood.X >= (x - Cosmos.foodTolerance)) &&
                (myFood.X <= (x + Cosmos.foodTolerance)) &&
                (myFood.Y >= (y - Cosmos.foodTolerance)) &&
                (myFood.Y <= (y + Cosmos.foodTolerance)))
            {
                foodCollected++;
         
                 invisible = true;
                //myWorld.NewFood(indMyFood);
            }
        }

       

        public void DrawToBuffer(Graphics g)
        {
            int tx, ty, bx, by, lx, ly, rx, ry;
            tx = (int)(Ants.AntTopX * Math.Cos(dir) - Ants.AntTopY * Math.Sin(dir) + x);
            ty = (int)(Ants.AntTopY * Math.Cos(dir) + Ants.AntTopX * Math.Sin(dir) + y);
            bx = (int)(Ants.AntBottomX * Math.Cos(dir) - Ants.AntBottomY * Math.Sin(dir) + x);
            by = (int)(Ants.AntBottomY * Math.Cos(dir) + Ants.AntBottomX * Math.Sin(dir) + y);
            lx = (int)(Ants.AntLeftX * Math.Cos(dir) - Ants.AntLeftY * Math.Sin(dir) + x);
            ly = (int)(Ants.AntLeftY * Math.Cos(dir) + Ants.AntLeftX * Math.Sin(dir) + y);
            rx = (int)(Ants.AntRightX * Math.Cos(dir) - Ants.AntRightY * Math.Sin(dir) + x);
            ry = (int)(Ants.AntRightY * Math.Cos(dir) + Ants.AntRightX * Math.Sin(dir) + y);
            //g.FillPolygon(Cosmos.whiteBrush, new Point[] { new Point(tx, ty), new Point(lx, ly), new Point(rx, ry) });
           // Rectangle rec = new Rectangle(tx, ty, lx - rx, by - ty);
           g.FillEllipse(Cosmos.whiteBrush, tx, ty, 2 * 6, 2 * 6);
            //g.FillEllipse(Cosmos.whiteBrush, rec);
            g.DrawString(foodCollected.ToString(), Cosmos.foodFont, Cosmos.foodCollectedB, (int)x + 5, (int)y + 8);
            g.DrawString(antNumber.ToString(), Cosmos.foodFont, Cosmos.antNumberB, (int)x + 5, (int)y - 20);
        }

        public static bool IsIntersected(PointF circle, float radius, RectangleF rectangle)
        {
            var rectangleCenter = new PointF((rectangle.X + rectangle.Width / 2),
                                             (rectangle.Y + rectangle.Height / 2));

            var w = rectangle.Width / 2;
            var h = rectangle.Height / 2;

            var dx = Math.Abs(circle.X - rectangleCenter.X);
            var dy = Math.Abs(circle.Y - rectangleCenter.Y);

            if (dx > (radius + w) || dy > (radius + h)) return false;

            var circleDistance = new PointF
            {
                X = Math.Abs(circle.X - rectangle.X - w),
                Y = Math.Abs(circle.Y - rectangle.Y - h)
            };

            if (circleDistance.X <= (w))
            {
                return true;
            }

            if (circleDistance.Y <= (h))
            {
                return true;
            }

            var cornerDistanceSq = Math.Pow(circleDistance.X - w, 2) +
                                            Math.Pow(circleDistance.Y - h, 2);

            return (cornerDistanceSq <= (Math.Pow(radius, 2)));
        }

        public int FoodCollected
        {
            get
            {
                return foodCollected;
            }
        }

        public PointF StartPosition
        {
            get
            {
                return startPosition;
            }
        }
        public PointF FinishPosition
        {
            get
            {
                return finishPosition;
            }
        }

        public double DistanceCovered
        {
            get
            {
                return Vector2.Length(Vector2.Subtract(new Vector2(startPosition), new Vector2((float)x, (float)y)));
            }
        }
       

        // Structure that stores the results of the PolygonCollision function
        public struct PolygonCollisionResult
        {
            public bool WillIntersect; // Are the polygons going to intersect forward in time?
            public bool Intersect; // Are the polygons currently intersecting
            public Vector2 MinimumTranslationVector; // The translation to apply to polygon A to push the polygons appart.
        }

        // Check if polygon A is going to collide with polygon B for the given velocity
        public PolygonCollisionResult PolygonCollision(Polygon polygonA, Polygon polygonB, Vector2 velocity)
        {
            PolygonCollisionResult result = new PolygonCollisionResult();
            result.Intersect = true;
            result.WillIntersect = true;

            int edgeCountA = polygonA.Edges.Count;
            int edgeCountB = polygonB.Edges.Count;
            float minIntervalDistance = float.PositiveInfinity;
            Vector2 translationAxis = new Vector2();
            Vector2 edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = polygonA.Edges[edgeIndex];
                }
                else
                {
                    edge = polygonB.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y + -25, edge.X + -10);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0) result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====

                // Project the velocity on the current axis
                float velocityProjection = axis.DotProduct(velocity);

                // Get the projection of polygon A during the movement
                if (velocityProjection < 0)
                {
                    minA += velocityProjection;
                }
                else
                {
                    maxA += velocityProjection;
                }

                // Do the same test as above for the new projection
                float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                if (!result.Intersect && !result.WillIntersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector2 d = polygonA.Center - polygonB.Center;
                    if (d.DotProduct(translationAxis) < 0) translationAxis = -translationAxis;
                }
            }

            // The minimum translation vector can be used to push the polygons appart.
            // First moves the polygons by their velocity
            // then move polygonA by MinimumTranslationVector.
            if (result.WillIntersect) result.MinimumTranslationVector = translationAxis * 3 * minIntervalDistance;

            return result;
        }

        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        public float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }

        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public void ProjectPolygon(Vector2 axis, Polygon polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float d = axis.DotProduct(polygon.Points[0]);
            min = d;
            max = d;
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                d = polygon.Points[i].DotProduct(axis);
                if (d < min)
                {
                    min = d;
                }
                else
                {
                    if (d > max)
                    {
                        max = d;
                    }
                }
            }
        }
    }
}
