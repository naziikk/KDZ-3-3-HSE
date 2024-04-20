namespace Botik;
public class Monument
{
    private string _id;
    private string _sculpName;
    private string _photo;
    private string _author;
    private string _manufactYear;
    private string _material;
    private string _description;
    private string _locationPlace;
    private string _longitudeWGS84;
    private string _latitudeWGS84;
    private string _globalId;
    private string _geoDataCenter;
    private string _geoArea;
    public string Id
    {
        get => _id;
        set => _id = value;
    }
    public string SculpName
    {
        get => _sculpName;
        set => _sculpName = value;
    }
    public string Photo
    {
        get => _photo;
        set => _photo = value;
    }
    public string Author
    {
        get => _author;
        set => _author = value;
    }
    public string ManufactYear
    {
        get => _manufactYear;
        set => _manufactYear = value;
    }
    public string Material
    {
        get => _material;
        set => _material = value;
    }
    public string Description
    {
        get => _description;
        set => _description = value;
    }
    public string LocationPlace
    {
        get => _locationPlace;
        set => _locationPlace = value;
    }
    public string LongitudeWGS84
    {
        get => _longitudeWGS84;
        set => _longitudeWGS84 = value;
    }
    public string LatitudeWGS84
    {
        get => _latitudeWGS84;
        set => _latitudeWGS84 = value;
    }
    public string GlobalId
    {
        get => _globalId;
        set => _globalId = value;
    }
    public string GeoDataCenter
    {
        get => _geoDataCenter;
        set => _geoDataCenter = value;
    }
    public string GeoArea
    {
        get => _geoArea;
        set => _geoArea = value;
    }
    public Monument(string id, string sculpName, string photo, string author, string manufactYear, string material,
        string description, string locationPlace, string longitudeWGS84, string latitudeWGS84,
        string globalId, string geoDataCenter, string geoArea)
    {
        _id = id;
        _sculpName = sculpName;
        _photo = photo;
        _author = author;
        _manufactYear = manufactYear;
        _material = material;
        _description = description;
        _locationPlace = locationPlace;
        _longitudeWGS84 = longitudeWGS84;
        _latitudeWGS84 = latitudeWGS84;
        _globalId = globalId;
        _geoDataCenter = geoDataCenter;
        _geoArea = geoArea;
    }
    public Monument()
    {
        _id = "";
        _sculpName = "";
        _photo = "";
        _author = "";
        _manufactYear = "";
        _material = "";
        _description = "";
        _locationPlace = "";
        _longitudeWGS84 = "";
        _latitudeWGS84 = "";
        _globalId = "";
        _geoDataCenter = "";
        _geoArea = "";
    }
}
