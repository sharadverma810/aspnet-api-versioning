﻿namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;
    using static System.ComponentModel.EditorBrowsableState;
    using static System.Globalization.CultureInfo;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Provides extension methods for the <see cref="ApiDescription"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// Gets the API version associated with the API description, if any.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the API version for.</param>
        /// <returns>The associated <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
        public static ApiVersion GetApiVersion( this ApiDescription apiDescription ) => apiDescription.GetProperty<ApiVersion>();

        /// <summary>
        /// Gets a value indicating whether the associated API description is deprecated.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to evaluate.</param>
        /// <returns><c>True</c> if the <see cref="ApiDescription">API description</see> is deprecated; otherwise, <c>false</c>.</returns>
        public static bool IsDeprecated( this ApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );

            var apiVersion = apiDescription.GetApiVersion();
            var model = apiDescription.ActionDescriptor.GetApiVersionModel( Explicit | Implicit );

            return model.DeprecatedApiVersions.Contains( apiVersion );
        }

        /// <summary>
        /// Sets the API version associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to set the API version for.</param>
        /// <param name="apiVersion">The associated <see cref="ApiVersion">API version</see>.</param>
        [EditorBrowsable( Never )]
        public static void SetApiVersion( this ApiDescription apiDescription, ApiVersion apiVersion ) => apiDescription.SetProperty( apiVersion );

        /// <summary>
        /// Attempts to update the relate path of the specified API description and remove the corresponding parameter according to the specified options.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to attempt to update.</param>
        /// <param name="options">The current <see cref="ApiExplorerOptions">API Explorer options</see>.</param>
        /// <returns>True if the <paramref name="apiDescription">API description</paramref> was updated; otherwise, false.</returns>
        public static bool TryUpdateRelativePathAndRemoveApiVersionParameter( this ApiDescription apiDescription, ApiExplorerOptions options )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Arg.NotNull( options, nameof( options ) );

            if ( !options.SubstituteApiVersionInUrl )
            {
                return false;
            }

            var parameter = apiDescription.ParameterDescriptions.FirstOrDefault( pd => pd.Source == Path && pd.ModelMetadata?.DataTypeName == nameof( ApiVersion ) );

            if ( parameter == null )
            {
                return false;
            }

            var relativePath = apiDescription.RelativePath;
            var token = '{' + parameter.Name + '}';
            var value = apiDescription.GetApiVersion().ToString( options.SubstitutionFormat, InvariantCulture );
            var newRelativePath = relativePath.Replace( token, value );

            if ( relativePath == newRelativePath )
            {
                return false;
            }

            apiDescription.RelativePath = newRelativePath;
            apiDescription.ParameterDescriptions.Remove( parameter );
            return true;
        }

        /// <summary>
        /// Creates a shallow copy of the current API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to create a copy of.</param>
        /// <returns>A new <see cref="ApiDescription">API description</see>.</returns>
        public static ApiDescription Clone( this ApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            Contract.Ensures( Contract.Result<ApiDescription>() != null );

            var clone = new ApiDescription()
            {
                ActionDescriptor = apiDescription.ActionDescriptor,
                GroupName = apiDescription.GroupName,
                HttpMethod = apiDescription.HttpMethod,
                RelativePath = apiDescription.RelativePath,
            };

            foreach ( var property in apiDescription.Properties )
            {
                clone.Properties.Add( property );
            }

            foreach ( var parameter in apiDescription.ParameterDescriptions )
            {
                clone.ParameterDescriptions.Add( parameter );
            }

            foreach ( var requestFormat in apiDescription.SupportedRequestFormats )
            {
                clone.SupportedRequestFormats.Add( requestFormat );
            }

            foreach ( var responseType in apiDescription.SupportedResponseTypes )
            {
                clone.SupportedResponseTypes.Add( responseType );
            }

            return clone;
        }

        internal static ApiRequestFormat Clone( this ApiRequestFormat requestFormat )
        {
            Contract.Requires( requestFormat != null );
            Contract.Ensures( Contract.Result<ApiRequestFormat>() != null );

            return new ApiRequestFormat()
            {
                Formatter = requestFormat.Formatter,
                MediaType = requestFormat.MediaType,
            };
        }

        internal static ApiResponseType Clone( this ApiResponseType responseType )
        {
            Contract.Requires( responseType != null );
            Contract.Ensures( Contract.Result<ApiResponseType>() != null );

            var clone = new ApiResponseType()
            {
                ModelMetadata = responseType.ModelMetadata,
                StatusCode = responseType.StatusCode,
                Type = responseType.Type,
            };

            foreach ( var responseFormat in responseType.ApiResponseFormats )
            {
                clone.ApiResponseFormats.Add( responseFormat.Clone() );
            }

            return clone;
        }

        static ApiResponseFormat Clone( this ApiResponseFormat responseFormat )
        {
            Contract.Requires( responseFormat != null );
            Contract.Ensures( Contract.Result<ApiResponseFormat>() != null );

            return new ApiResponseFormat()
            {
                Formatter = responseFormat.Formatter,
                MediaType = responseFormat.MediaType,
            };
        }
    }
}