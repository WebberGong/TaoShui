namespace WebSite
{
    public enum WebSiteState
    {
        Start,
        Logging,
        CaptchaValidating,
        LoginSuccessful,
        LoginFailed,
        End
    }
}