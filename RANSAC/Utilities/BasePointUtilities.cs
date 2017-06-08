﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RANSAC.Structures;
using Emgu.CV.Features2D;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Drawing;

namespace RANSAC.Utilities
{
    class BasePointUtilities
    {
        public static ImageFeature<float>[] getImageFeatures(Bitmap bitmap)
        {
            Image<Gray, Byte> image = new Image<Gray, Byte>(bitmap);
            var detector = new SIFTDetector();
            return detector.DetectFeatures(image, null);
        }

        private List<FPoint> getKeyPoints(ImageFeature<float>[] features)
        {
            List<FPoint> keyPointList = new List<FPoint>();
            foreach (var feature in features)
            {
                var pointCoordinatesAndFeatures = feature.Descriptor;
                var pointFeatures = pointCoordinatesAndFeatures.Skip(5).ToArray();
                keyPointList.Add(new FPoint(pointCoordinatesAndFeatures[0], pointCoordinatesAndFeatures[1], pointFeatures));
            }
            return keyPointList;
        }

        public static List<Tuple<FPoint, FPoint>> getMutualPointsPairs(List<FPoint> firstKeyPoints, List<FPoint> secondKeyPoints)
        {
            List<Tuple<FPoint, FPoint>> mutualPoints = new List<Tuple<FPoint, FPoint>>();

            foreach (var point in firstKeyPoints)
            {
                double distance = double.MaxValue;
                FPoint secondPoint = null;

                for (int i = 0; i < secondKeyPoints.Count; i++)
                {
                    var p = secondKeyPoints.ElementAt(i);
                    double dist = point.featuresDistance(secondPoint);
                    if (dist < distance)
                    {
                        distance = dist;
                        secondPoint = p;

                    }
                }

                if (isMutual(secondPoint, point, firstKeyPoints))
                {
                    mutualPoints.Add(new Tuple<FPoint, FPoint>(point, secondPoint));
                }
            }

            return mutualPoints;
        }

        private static bool isMutual(FPoint foundPoint, FPoint givenPoint, List<FPoint> points)
        {
            double distance = double.MaxValue;
            FPoint p = null;
            foreach (var point in points)
            {
                double dist = foundPoint.featuresDistance(point);
                if (dist < distance)
                {
                    distance = dist;
                    p = point;
                }
            }

            return p.Equals(givenPoint);
        }

        public static List<Tuple<FPoint, FPoint>> neighbourFilter(List<Tuple<FPoint, FPoint>> keyPointsPairs, int neighbours, double threshold)
        {
            List<Tuple<FPoint, FPoint>> points = new List<Tuple<FPoint, FPoint>>();

            foreach (var pair in keyPointsPairs)
            {
                var firstPoint = pair.Item1;
                var secondPoint = pair.Item2;

                var closestToFirstPoint = keyPointsPairs.OrderBy(x => Math.Sqrt(Math.Pow(x.Item1.X - firstPoint.X, 2) + Math.Pow(x.Item1.Y - firstPoint.Y, 2))).Take(neighbours);
                var closestToSecondPoint = keyPointsPairs.OrderBy(x => Math.Sqrt(Math.Pow(x.Item2.X - secondPoint.X, 2) + Math.Pow(x.Item2.Y - secondPoint.Y, 2))).Take(neighbours);

                int pointsInNeighbourHood = closestToFirstPoint.Count(x => closestToSecondPoint.Contains(x));
                double neighbourhoodQuality = (double)pointsInNeighbourHood / (double)neighbours;

                if (neighbourhoodQuality >= threshold)
                {
                    points.Add(pair);
                }
            }

            return points;
        }
    }
}