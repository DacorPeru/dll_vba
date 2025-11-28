Imports System.Runtime.InteropServices

' Hacemos visible la clase para VBA y otros lenguajes COM
<ComVisible(True)>
<ProgId("DacorPeru.StrongKey")>
Public Class StrongKeyInfo

    ' ✅ Devuelve el nombre de la empresa
    Public Function GetCompanyName() As String
        Return "Data Consulting Reyes Perú E.I.R.L."
    End Function

    ' ✅ Devuelve el propietario o responsable
    Public Function GetOwner() As String
        Return "Gesmer Biuler Reyes Eustaquio"
    End Function

    ' ✅ Devuelve la versión de la firma
    Public Function GetSignatureVersion() As String
        Return "Dacor Perú – Firma empresarial v1.0 (2025)"
    End Function

End Class
