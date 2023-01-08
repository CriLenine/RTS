using RTS.Feedback;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[System.Serializable]
[GameFeedback(51, 70, 30, "ShootProjectile")]
public class ShootProjectile : GameFeedback
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed;
    protected override void Execute(GameObject gameObject)
    {
        if (!gameObject.TryGetComponent(out Character character)) return;

        if(character.Data.Type != Character.Type.Bowman) return;

        GameObject projectile = GameObject.Instantiate(_projectilePrefab, gameObject.transform.position, Quaternion.identity);

        character.Shoot(projectile, _projectileSpeed);
    }

    public override string ToString()
    {
        return $"Shootprojectile {(_projectilePrefab == null ? "" : _projectilePrefab.name)}";
    }

}
