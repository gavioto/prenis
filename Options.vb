Public Class Options

    Private m_useRecursiveReferencing As Boolean = True
    Public Property UseRecursiveReferencing() As Boolean
        Get
            Return m_useRecursiveReferencing
        End Get
        Set(ByVal Value As Boolean)
            m_useRecursiveReferencing = Value
        End Set
    End Property

    Private m_useProjectBinaries As Boolean = True
    Public Property UseProjectBinaries() As Boolean
        Get
            Return m_useProjectBinaries
        End Get
        Set(ByVal value As Boolean)
            m_useProjectBinaries = value
        End Set
    End Property

    Private m_useReferenceBinaries As Boolean = True
    Public Property UseReferenceBinaries() As Boolean
        Get
            Return m_useReferenceBinaries
        End Get
        Set(ByVal value As Boolean)
            m_useReferenceBinaries = value
        End Set
    End Property

End Class
