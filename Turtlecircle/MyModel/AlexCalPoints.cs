﻿using System;
namespace turtletest
{
	public class AlexCalPoints
	{
		public AlexCalPoints()
		{
		}

		public static PointF[] MakeStarPoints(double start_theta, int num_points, int skip, Rectangle rect)
		{
			double theta, dtheta;
			PointF[] result;
			float rx = rect.Width / 2f;
			float ry = rect.Height / 2f;
			float cx = rect.X + rx;
			float cy = rect.Y + ry;

			// If this is a polygon, don't bother with concave points.
			if (skip == 1)
			{
				result = new PointF[num_points];
				theta = start_theta;
				dtheta = 2 * Math.PI / num_points;
				for (int i = 0; i < num_points; i++)
				{
					result[i] = new PointF(
						(float)(cx + rx * Math.Cos(theta)),
						(float)(cy + ry * Math.Sin(theta)));
					theta += dtheta;
				}
				return result;
			}

			// Find the radius for the concave vertices.
			double concave_radius = CalculateConcaveRadius(num_points, skip);

			// Make the points.
			result = new PointF[2 * num_points];
			theta = start_theta;
			dtheta = Math.PI / num_points;
			for (int i = 0; i < num_points; i++)
			{
				result[2 * i] = new PointF(
					(float)(cx + rx * Math.Cos(theta)),
					(float)(cy + ry * Math.Sin(theta)));
				theta += dtheta;
				result[2 * i + 1] = new PointF(
					(float)(cx + rx * Math.Cos(theta) * concave_radius),
					(float)(cy + ry * Math.Sin(theta) * concave_radius));
				theta += dtheta;
			}
			return result;
		}

		// Calculate the inner star radius.
		private static double CalculateConcaveRadius(int num_points, int skip)
		{
			// For really small numbers of points.
			if (num_points < 5) return 0.33f;

			// Calculate angles to key points.
			double dtheta = 2 * Math.PI / num_points;
			double theta00 = -Math.PI / 2;
			double theta01 = theta00 + dtheta * skip;
			double theta10 = theta00 + dtheta;
			double theta11 = theta10 - dtheta * skip;

			// Find the key points.
			PointF pt00 = new PointF(
				(float)Math.Cos(theta00),
				(float)Math.Sin(theta00));
			PointF pt01 = new PointF(
				(float)Math.Cos(theta01),
				(float)Math.Sin(theta01));
			PointF pt10 = new PointF(
				(float)Math.Cos(theta10),
				(float)Math.Sin(theta10));
			PointF pt11 = new PointF(
				(float)Math.Cos(theta11),
				(float)Math.Sin(theta11));

			// See where the segments connecting the points intersect.
			bool lines_intersect, segments_intersect;
			PointF intersection, close_p1, close_p2;
			FindIntersection(pt00, pt01, pt10, pt11,
				out lines_intersect, out segments_intersect,
				out intersection, out close_p1, out close_p2);

			// Calculate the distance between the
			// point of intersection and the center.
			return Math.Sqrt(
				intersection.X * intersection.X +
				intersection.Y * intersection.Y);
		}

		// Find the point of intersection between
		// the lines p1 --> p2 and p3 --> p4.
		private static void FindIntersection(
			PointF p1, PointF p2, PointF p3, PointF p4,
			out bool lines_intersect, out bool segments_intersect,
			out PointF intersection,
			out PointF close_p1, out PointF close_p2)
		{
			// Get the segments' parameters.
			float dx12 = p2.X - p1.X;
			float dy12 = p2.Y - p1.Y;
			float dx34 = p4.X - p3.X;
			float dy34 = p4.Y - p3.Y;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 =
				((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
					/ denominator;
			if (float.IsInfinity(t1))
			{
				// The lines are parallel (or close enough to it).
				lines_intersect = false;
				segments_intersect = false;
				intersection = new PointF(float.NaN, float.NaN);
				close_p1 = new PointF(float.NaN, float.NaN);
				close_p2 = new PointF(float.NaN, float.NaN);
				return;
			}
			lines_intersect = true;

			float t2 =
				((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
					/ -denominator;

			// Find the point of intersection.
			intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segments_intersect =
				((t1 >= 0) && (t1 <= 1) &&
				 (t2 >= 0) && (t2 <= 1));

			// Find the closest points on the segments.
			if (t1 < 0)
			{
				t1 = 0;
			}
			else if (t1 > 1)
			{
				t1 = 1;
			}

			if (t2 < 0)
			{
				t2 = 0;
			}
			else if (t2 > 1)
			{
				t2 = 1;
			}

			close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
			close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
		}
	}
}
