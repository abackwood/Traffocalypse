using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SourceSink : Connection {
	static List<SourceSink> sources, sinks;

	public static SourceSink[] Sources {
		get { return sources.ToArray(); }
	}
	public static SourceSink[] Sinks {
		get { return sinks.ToArray(); }
	}

	public Road road;
	public bool source, sink;

	// Use this for initialization
	void Start () {
		if(sources == null) sources = new List<SourceSink>();
		if(sinks == null) sinks = new List<SourceSink>();

		if(source) sources.Add (this);
		if(sink) sinks.Add (this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
