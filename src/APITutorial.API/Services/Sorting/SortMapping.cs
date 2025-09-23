﻿namespace APITutorial.API.Services.Sorting;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);


public interface ISortMappingDefinition;
