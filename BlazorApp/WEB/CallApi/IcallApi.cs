namespace WEB.CallApi
{
    public interface IcallApi
    {
       Task<string> PostAsync(string url, object body = null, string bearerToken = null);
    }
}
