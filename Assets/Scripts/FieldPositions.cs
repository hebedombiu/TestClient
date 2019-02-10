using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldPositions : IEnumerable<FieldPosition> {
    private List<FieldPosition> _positions = new List<FieldPosition>();

    public void Add(FieldPosition position) {
        _positions.Add(position);
    }

    public IEnumerator<FieldPosition> GetEnumerator() {
        return _positions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

public class FieldPosition {
    private FieldPositionType _type;
    private int _id;
    private Vector2 _position;

    public FieldPosition(FieldPositionType type, Vector2 position, int id = -1) {
        _type = type;
        _position = position;
        _id = id;
    }

    public FieldPositionType Type => _type;
    public Vector2 Position => _position;
    public int Id => _id;
}

public enum FieldPositionType {
    Player,
    Food
}