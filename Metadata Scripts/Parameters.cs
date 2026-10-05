using FModBankParser.Nodes;
using System.Xml;
using static Program;
using static XMLHelper;

public class Parameters
{
	// FMOD Studio writes only non-default values on a GameParameter
	private const float DefaultInitialValue = 0f;
	private const float DefaultMinimum = 0f;
	private const float DefaultMaximum = 1f;

	private struct ParamFlags(string type)
	{
		public string Type { get; set; } = type;
		public bool IsGlobal { get; set; } = false;
		public bool IsReadOnly { get; set; } = false;
		public bool IsHeld { get; set; } = false;
	}

	public static void ParameterXML(ParameterNode Param)
	{
		string dedupKey = Param.BaseGuid.ToString();
		lock (EmittedParameterPresets)
		{
			if (!EmittedParameterPresets.Add(dedupKey))
				return;
		}
		Guid XMLGUID = GetRandomGUID();
		// Remember the preset GUID so ParameterProxy.preset can link to it
		ParameterPresetGUIDs.TryAdd(Param.BaseGuid, XMLGUID);

		#region Preset Segment
		SetupXML(out XmlDocument xmlDoc, out XmlElement root);
		SetupHeaderXML(xmlDoc, root, "ParameterPreset", $"{{{XMLGUID}}}", out XmlElement PresetElement);

		AddPropertyElement(xmlDoc, PresetElement, "name", Param.Name);
		// Every preset in the original project lives inside a real ParameterPresetFolder, not directly under the master folder
		AddRelationshipElement(xmlDoc, PresetElement, "folder", $"{{{ParameterPresetFolderGUID}}}");
		AddRelationshipElement(xmlDoc, PresetElement, "parameter", $"{{{Param.BaseGuid}}}");
		#endregion

		#region Parameter Segment
		SetupHeaderXML(xmlDoc, root, "GameParameter", $"{{{Param.BaseGuid}}}", out XmlElement ParamElement);

		#region Resolve Parameter Types
		ParamFlags ParamData = new((Param.Flags & 0x28) switch
		{
			0x20 or 0x28 => "Labeled",
			0x08 => "Discrete",
			_ => "Continuous"
		})
		{
			IsGlobal = (Param.Flags & 0x01) != 0,
			IsHeld = (Param.Flags & 0x02) != 0,
			IsReadOnly = (Param.Flags & 0x10) != 0
		};

		int BuiltinParamType = (uint)Param.Type switch
		{
			0x1 => 3, // Distance
			0x4 => 4, // Direction
			0x5 => 5, // Elevation
			0x2 => 6, // Event Cone Angle
			0x3 => 7, // Event Orientation
			0x7 => 8, // Speed (Relative)
			0x8 => 9, // Speed (Absolute)
			0x9 => 10, // Distance (Normalized)
			_ => -1, // GAME CONTROLLED has no built-in type
		};
		#endregion

		bool isLabeled = Param.Labels.Length > 0 && ParamData.Type == "Labeled";
		bool isDiscrete = !isLabeled && ParamData.Type == "Discrete";

		/*
			0 = continuous (default, omitted)
			1 = discrete
			2 = labeled
		*/
		int effectiveType = isLabeled ? 2 : (isDiscrete ? 1 : (BuiltinParamType != -1 ? BuiltinParamType : 0));
		if (effectiveType != 0)
			AddPropertyElement(xmlDoc, ParamElement, "parameterType", $"{effectiveType}");

		if (isLabeled)
			AddMultiPropertyElement(xmlDoc, ParamElement, "enumerationLabels", Param.Labels);

		/*
			Range. For a labeled parameter Studio stores the LABEL COUNT (verified: the examples
			`Surface` parameter has 3 labels and stores maximum=3, while the bank holds max=2).
			For everything else the bank range is used directly.
		*/
		float min = Param.Minimum;
		float max = isLabeled ? Param.Labels.Length : Param.Maximum;
		if (min != DefaultMinimum)
			AddPropertyElement(xmlDoc, ParamElement, "minimum", $"{min}");
		if (max != DefaultMaximum)
			AddPropertyElement(xmlDoc, ParamElement, "maximum", $"{max}");

		if (Param.DefaultValue != DefaultInitialValue)
			AddPropertyElement(xmlDoc, ParamElement, "initialValue", $"{Param.DefaultValue}");

		if (ParamData.IsGlobal)
			AddPropertyElement(xmlDoc, ParamElement, "isGlobal", "true");
		if (ParamData.IsReadOnly)
			AddPropertyElement(xmlDoc, ParamElement, "isReadOnly", "true");
		if (ParamData.IsHeld)
			AddPropertyElement(xmlDoc, ParamElement, "isHeld", "true");

		if (Param.Velocity != 0)
			AddPropertyElement(xmlDoc, ParamElement, "velocity", $"{Param.Velocity}");
		if (Param.SeekSpeed != 0)
			AddPropertyElement(xmlDoc, ParamElement, "seekSpeed", $"{Param.SeekSpeed}");
		if (Param.SeekSpeedDown != 0)
			AddPropertyElement(xmlDoc, ParamElement, "seekSpeedDescending", $"{Param.SeekSpeedDown}");
		#endregion

		xmlDoc.AppendChild(root);

		SaveXML(xmlDoc, $"{outputProjectPath}/Metadata/ParameterPreset/{{{XMLGUID}}}.xml");
	}
}
