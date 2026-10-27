using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Animals.Movement;
using Object = UnityEngine.Object;

namespace ZooWorld.Tests.Support
{
    /// <summary>
    /// Builds species and animals the same way the assets and prefabs are wired,
    /// and destroys everything it created on dispose.
    /// </summary>
    internal sealed class AnimalBuilder : IDisposable
    {
        private readonly List<Object> _created = new();
        private readonly IAnimalEncounterHandler _encounterHandler;

        public AnimalBuilder(IAnimalEncounterHandler encounterHandler = null)
        {
            _encounterHandler = encounterHandler;
        }

        public AnimalConfig Species(AnimalRole role)
        {
            var config = ScriptableObject.CreateInstance<AnimalConfig>();
            config.name = role.ToString();
            SetFields(config, ("_role", property => property.enumValueIndex = (int)role));
            _created.Add(config);
            return config;
        }

        public void SetDiet(AnimalConfig species, params AnimalConfig[] food)
        {
            SetFields(species, ("_diet", property =>
            {
                property.ClearArray();
                for (var i = 0; i < food.Length; i++)
                {
                    property.InsertArrayElementAtIndex(i);
                    property.GetArrayElementAtIndex(i).objectReferenceValue = food[i];
                }
            }));
        }

        public Animal Create(AnimalRole role) => Create(Species(role));

        public Animal Alive(AnimalConfig species)
        {
            var animal = Create(species);
            animal.Spawn(Vector3.zero, Vector3.forward);
            return animal;
        }

        public Animal Create(AnimalConfig species)
        {
            var gameObject = new GameObject(species.name);
            gameObject.SetActive(false);
            var body = gameObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            var movement = gameObject.AddComponent<LinearMovement>();
            var animal = gameObject.AddComponent<Animal>();

            SetFields(movement, ("_body", property => property.objectReferenceValue = body));
            SetFields(animal,
                ("_config", property => property.objectReferenceValue = species),
                ("_movement", property => property.objectReferenceValue = movement),
                ("_body", property => property.objectReferenceValue = body));

            movement.Construct(new FakePlayArea());
            animal.Construct(_encounterHandler);

            _created.Add(gameObject);
            return animal;
        }

        public void Dispose()
        {
            foreach (var created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        private static void SetFields(Object target, params (string Name, Action<SerializedProperty> Set)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var (name, set) in fields)
            {
                set(serialized.FindProperty(name));
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
