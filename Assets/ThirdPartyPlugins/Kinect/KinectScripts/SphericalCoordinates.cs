using UnityEngine;

public class SphericalCoordinates
{
	// Determine what happen when a limit is reached, repeat or clamp.
	public bool 	_loopPolar 			= true;
	public bool 	_loopElevation 		= false;
	private float 	_radius;
	private float 	_polarAngle;
	private float 	_elevationAngle;
	private float 	_minRadius;
	private float 	_maxRadius;
	private float 	_minPolarAngle;
	private float 	_maxPolarAngle;
	private float 	_minElevationAngle;
	private float 	_maxElevationAngle;

	public float Radius
	{ 
		get
		{
			return _radius;
		}

		private set
		{
			_radius = Mathf.Clamp(value, _minRadius, _maxRadius);
		}
	}

	public float PolarAngle
	{ 
		get
		{
			return _polarAngle;
		}

		private set
		{ 
			_polarAngle = _loopPolar ? Mathf.Repeat(value, _maxPolarAngle - _minPolarAngle) : Mathf.Clamp(value, _minPolarAngle, _maxPolarAngle); 
		}
	}

	public float ElevationAngle
	{ 
		get
		{
			return _elevationAngle;
		}

		private set
		{ 
			_elevationAngle = _loopElevation ? Mathf.Repeat(value, _maxElevationAngle - _minElevationAngle) : Mathf.Clamp(value, _minElevationAngle, _maxElevationAngle); 
		}
	}

	public SphericalCoordinates()
	{
	}

	public SphericalCoordinates(
		float r,
		float p,
		float s,
		float minRadius = 1f,
		float maxRadius = 20f,
		float minPolar = 0f,
		float maxPolar = (Mathf.PI*2f),
		float minElevation = 0f,
		float maxElevation = (Mathf.PI/3f))
	{
		_minRadius = minRadius;
		_maxRadius = maxRadius;
		_minPolarAngle = minPolar;
		_maxPolarAngle = maxPolar;
		_minElevationAngle = minElevation;
		_maxElevationAngle = maxElevation;

		SetRadius(r);
		SetRotation(p, s);
	}

	public SphericalCoordinates(
		Transform T,
		float minRadius = 1f,
		float maxRadius = 20f,
		float minPolar = 0f,
		float maxPolar = (Mathf.PI*2f),
		float minElevation = 0f,
		float maxElevation = (Mathf.PI/3f))
		: this(T.position, minRadius, maxRadius, minPolar, maxPolar, minElevation, maxElevation) 
	{
	}

	public SphericalCoordinates(
		Vector3 cartesianCoordinate,
		float minRadius = 1f,
		float maxRadius = 20f,
		float minPolar = 0f,
		float maxPolar = (Mathf.PI*2f),
		float minElevation = 0f,
		float maxElevation = (Mathf.PI/3f))
	{
		_minRadius = minRadius;
		_maxRadius = maxRadius;
		_minPolarAngle = minPolar;
		_maxPolarAngle = maxPolar;
		_minElevationAngle = minElevation;
		_maxElevationAngle = maxElevation;

		FromCartesian(cartesianCoordinate);
	}

	public Vector3 Cartesian
	{
		get
		{
			float a = Radius * Mathf.Cos(ElevationAngle);
			return new Vector3(a * Mathf.Cos(PolarAngle), Radius * Mathf.Sin(ElevationAngle), a * Mathf.Sin(PolarAngle));
		}
	}

	public SphericalCoordinates FromCartesian(Vector3 cartesianCoordinate)
	{
		if (cartesianCoordinate.x == 0f)
		{
			cartesianCoordinate.x = Mathf.Epsilon;
		}

		Radius = cartesianCoordinate.magnitude;
		PolarAngle = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

		if (cartesianCoordinate.x < 0f)
		{
			PolarAngle += Mathf.PI;
		}

		ElevationAngle = Mathf.Asin(cartesianCoordinate.y / Radius);

		return this;
	}

	public SphericalCoordinates RotatePolarAngle(float x)
	{
		return Rotate(x, 0f); 
	}

	public SphericalCoordinates RotateElevationAngle(float x)
	{
		return Rotate(0f, x);
	}

	public SphericalCoordinates Rotate(float newPolar, float newElevation)
	{
		return SetRotation(PolarAngle + newPolar, ElevationAngle + newElevation );
	}

	public SphericalCoordinates SetPolarAngle(float x)
	{
		return SetRotation(x, ElevationAngle);
	}

	public SphericalCoordinates SetElevationAngle(float x)
	{
		return SetRotation(x, ElevationAngle);
	}

	public SphericalCoordinates SetRotation(float newPolar, float newElevation)
	{
		PolarAngle = newPolar;		
		ElevationAngle = newElevation;
		return this;
	}

	public SphericalCoordinates TranslateRadius(float x)
	{
		return SetRadius(Radius + x);
	}

	public SphericalCoordinates SetRadius(float rad)
	{
		Radius = rad;
		return this;
	}

	public override string ToString()
	{
		return "[Radius] " + Radius + ". [Polar] " + PolarAngle + ". [Elevation] " + ElevationAngle + ".";
	}
}