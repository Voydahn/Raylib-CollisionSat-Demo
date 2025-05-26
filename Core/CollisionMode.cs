namespace CollisionSatSystem.Core;

public enum CollisionMode
{
    /// <summary>
    /// Mode de collision basé sur l'algorithme SAT (Separating Axis Theorem).
    /// Utilisé pour les collisions entre polygones convexes.
    /// </summary>
    SAT,

    /// <summary>
    /// Mode de collision basé sur la détection d'intersection des diagonales.
    /// Utilisé pour les collisions entre polygones non convexes ou irréguliers.
    /// </summary>
    DIAGS
}