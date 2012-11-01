namespace IDS_ADI.InformationModel
{
  #region Enumerations
  enum InformationModelWorksheets
  {
    Class = 1,
    ClassSpecialization,
    Classifications,
    SpecializedIndividualTemplate,
    ClassTemplateInstance,
    BaseTemplate,
  }

  enum ClassColumns
  {
    Load,
    Note,
    Reference1,
    Reference2,
    ID,
    Label,
    Description,
    EntityType,
  }

  enum ClassificationColumns
  {
      Load,
      Note,
      Class,
      Classified
  }
  enum ClassSpecializationColumns
  {
    Load,
    Note,
    Superclass,
    Subclass,
  }

  enum TemplateColumns
  {
    Load,
    Note,
    ClassLabel,
    Property,
    RelationshipTo,
    RelType,
    RelCardinality,
    ID,
    Name,
    Description,
    ParentTemplate,
    Roles,
  }

  enum RoleColumns
  {
    ID,
    Name,
    Description,
    Type,
    Value,
    Count,
    Max = 7,
  }
  #endregion
}
