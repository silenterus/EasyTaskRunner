namespace EasyTaskRunner.Data.Utilities;

public class ErrorCounter
{

    private bool IsActive { get; set; } = false;
    private readonly Dictionary<string, ErrorValue> _values = new Dictionary<string, ErrorValue>();

    private readonly Dictionary<int, ErrorValue> _valuesTest = new Dictionary<int, ErrorValue>();

    public ErrorCounter()
    {
        IsActive = false;
    }

    public ErrorCounter(string name, int maxError = 1)
    {
        if (maxError < 1)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        _values[name] = new ErrorValue(maxError);
        IsActive = _values.Any();
    }

    public ErrorCounter(string[] names, int maxError = 1)
    {
        if (maxError < 1)
        {
            return;
        }
        foreach (var name in names)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _values[name] = new ErrorValue(maxError);
            }
        }

        IsActive = _values.Any();
    }

    public ErrorCounter((string name, int maxError)[] errors)
    {
        foreach (var error in errors)
        {
            if (!string.IsNullOrWhiteSpace(error.name) && error.maxError > 0)
            {
                _values[error.name] = new ErrorValue(error.maxError);
            }
        }

        IsActive = _values.Any();
    }

    public bool ReachedAll()
    {
        if (!IsActive)
        {
            return false;
        }

        return _values.Values.Any(v => v.IsLimit());
    }


    public bool Reached(string[] errorNames)
    {
        if (!IsActive)
        {
            return false;
        }
        foreach (string name in errorNames)
        {
            if(Reached(name))
            {
                return true;
            }
        }

        return false;
    }

    public bool Reached(string errorName)
    {
        if (!IsActive)
        {
            return false;
        }

        return _values.TryGetValue(errorName, out ErrorValue? value) && value.IsLimit();
    }

    public void ErrorAll()
    {
        if (!IsActive)
        {
            return;
        }

        foreach (var value in _values.Values)
        {
            value.IncrementError();
        }
    }

    public void Error(string[] errorNames)
    {
        if (!IsActive)
        {
            return;
        }
        foreach (string name in errorNames)
        {
            Error(name);
        }
    }

    public void Error(string errorName)
    {
        if (!IsActive)
        {
            return;
        }

        if (_values.TryGetValue(errorName, out ErrorValue? value))
        {
            value.IncrementError();
        }
    }

    public void ClearAll()
    {
        if (!IsActive)
        {
            return;
        }

        foreach (var value in _values.Values)
        {
            value.Reset();
        }

    }

    public void Clear(string clearName)
    {
        if (!IsActive)
        {
            return;
        }
        if (_values.TryGetValue(clearName, out ErrorValue? value))
        {
            value.Reset();
        }
    }

    public void Clear(string[] clearNames)
    {
        if (!IsActive)
        {
            return;
        }
        foreach (string name in clearNames)
        {
            Clear(name);
        }
    }


    public void SetErrorLimit(string name, int maxError)
    {
        if (maxError < 1 || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (_values.ContainsKey(name))
        {
            _values[name] = new ErrorValue(maxError);
        }
        else
        {
            _values.Add(name, new ErrorValue(maxError));
        }
    }
}
