using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLineAnimator : MonoBehaviour
{
  private LineRenderer lineRenderer;
  private ParticleSystem lineParticle;

  private List<Vector3> points;
  private Vector3 nextPoint;
  private int step;
  private float lerpCoef;

	void Start()
  {
    lineRenderer = this.gameObject.transform.Find("line").GetComponent<LineRenderer>();
    lineParticle = lineRenderer.gameObject.transform.Find("line_particle").GetComponent<ParticleSystem>();
	}
	
	void Update()
  {
    if (this.points != null)
    {
      lerpCoef += Time.deltaTime * 8.0f;
      bool needFinish = false;
      if (lerpCoef >= 1.0f)
      {
        lerpCoef = 0.0f;
        if (step == 0)
        {
          this.points[this.points.Count - 1] = nextPoint;
          this.points.Add(nextPoint);
          nextPoint = new Vector3(-7.5f, -10.0f, 0.5f);
          step++;
        }
        else if (step == 1)
        {
          this.points[this.points.Count - 1] = nextPoint;
          this.points.Add(nextPoint);
          nextPoint = new Vector3(7.5f, -10.0f, 0.5f);
          step++;
        }
        else if (step == 2)
        {
          this.points[this.points.Count - 1] = nextPoint;
          this.points.Add(nextPoint);
          nextPoint = new Vector3(7.5f, 10.0f, 0.5f);
          step++;
        }
        else if (step == 3)
        {
          needFinish = true;
        }
        else
        {
          lerpCoef = 1.0f;
        }
      }

      if (!needFinish)
      {
        int index1 = this.points.Count - 2;
        int index2 = this.points.Count - 1;
        this.points[index2] = Vector3.Lerp(this.points[index1], nextPoint, lerpCoef);
        lineParticle.transform.localPosition = this.points[index2];
      }
      else
      {
        this.points.RemoveAt(this.points.Count - 1);
        lineRenderer.loop = true;
        lineParticle.gameObject.SetActive(false);
      }
        
      lineRenderer.positionCount = this.points.Count;
      lineRenderer.SetPositions(this.points.ToArray());
      if (needFinish)
        this.points = null;
    }
	}

  public void RunAnimation()
  {
    if (this.points != null)
      return;
    
    lineRenderer.gameObject.SetActive(true);
    lineParticle.gameObject.SetActive(true);
    this.points = new List<Vector3>();
    this.points.Add(new Vector3(7.5f, 10.0f, 0.5f));
    this.points.Add(new Vector3(7.5f, 10.0f, 0.5f));
    nextPoint = new Vector3(-7.5f, 10.0f, 0.5f);
    step = 0;
    lerpCoef = 0.0f;

    lineRenderer.positionCount = this.points.Count;
    lineRenderer.SetPositions(this.points.ToArray());
    lineRenderer.loop = false;
  }

  public void StopAnimation()
  {
    lineRenderer.gameObject.SetActive(false);
  }
}
