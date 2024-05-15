using Godot;
using System;

/// <summary>
/// </summary>
public partial class BeamScript : Node2D
{
	public Vector2 PositionA {
		get { 
			return positionA; 
		} 
		set {
			positionA = value;
			RecalculateBeam();
		} 
	}
	private Vector2 positionA = new Vector2(0,0);


	public Vector2 PositionB {
		get {
			return positionB;
		}
		set {
			positionB = value;
			RecalculateBeam();
		}
	}
	private Vector2 positionB = new Vector2(100,100);

	public float Width {
		get {
			return width;
		}
		set {
			width = value;
			RecalculateBeam();
		}
	}
	private float width = 5f;

	public Color Color { 
		get { 
			return color; 
		} 
		set { 
			color = value; 
			QueueRedraw(); 
		} 
	}
	private Color color = new Color(1f, 0.3f, 0.3f, 0.7f);


	private int animationState = 0;
	private float timer = 0;

	public override void _Ready()
	{
		RecalculateBeam();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		timer += (float)delta;

		if (timer > 0.1f) {
			animationState = -animationState + 1;
			QueueRedraw();
		}
	}

	/// <summary>
	/// Called when position is changed to recalculate collision box and line drawing.
	/// </summary>
	private void RecalculateBeam() {
		//initialize points as a 1D line, then adjust from there.
		Vector2[] points = { new Vector2(PositionA.X, PositionA.Y),
							 new Vector2(PositionA.X, PositionA.Y),
							 new Vector2(PositionB.X, PositionB.Y),
							 new Vector2(PositionB.X, PositionB.Y)};

		Vector2 widthDirection = (PositionB - PositionA).Normalized().Rotated(MathF.PI/2);
		points[0] += widthDirection * width / 2;
		points[1] -= widthDirection * width / 2;
		points[2] -= widthDirection * width / 2;
		points[3] += widthDirection * width / 2;
		//Compiler probably cleans up the above, idk. Easier to read this way.

		GetNode<CollisionPolygon2D>("Beam/Hitbox").Polygon = points;

		QueueRedraw();
	}

	public override void _Draw() {
		DrawLine(positionA, positionB, color, width - 1 + 2*animationState);
	}
}
