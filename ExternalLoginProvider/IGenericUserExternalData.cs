namespace ColibriWebApi.ExternalLoginProvider
{
    public interface IGenericUserExternalData
    {
        string UserName { get;  }
        string Name { get;  }
        string Email { get;  }
        string LastName { get;  }
        string UserPicture { get;  }
        string ExternalProviderId { get;  }
    }
}
