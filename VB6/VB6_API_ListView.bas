Option Explicit On

Private Type Produto
    Nome  As String
    Preco As Double
End Type

Private Sub cmdCarregar_Click()
    Dim produtos() As Produto
    Dim total As Long

    If CarregarProdutos(produtos, total) Then
        ExibirProdutos produtos, total
    Else
        MsgBox "Falha ao carregar produtos da API.", vbCritical, "Erro"
    End If
End Sub

Private Sub Form_Load()
    ConfigurarListView()
End Sub

Private Sub ConfigurarListView()
    With ListView1
        .View = lvwReport
        .FullRowSelect = True
        .GridLines = True
        .ColumnHeaders.Clear

        .ColumnHeaders.Add , , "Nome do Produto", 200
        .ColumnHeaders.Add , , "Preço R$ ", 120
    End With
End Sub

Private Function CarregarProdutos(ByRef produtos() As Produto,
                                   ByRef total As Long) As Boolean
    Const API_URL As String = "https://api.exemplo.com/produtos"

    Dim oHttp As Object
    Dim sResp As String

    On Error GoTo ErrHandler

    Set oHttp = CreateObject("MSXML2.XMLHTTP.6.0")
    oHttp.Open "GET", API_URL, False     
    oHttp.setRequestHeader "Accept", "application/json"
    oHttp.Send

    If oHttp.Status <> 200 Then
        MsgBox "HTTP " & oHttp.Status & ": " & oHttp.statusText,
               vbExclamation, "Erro na API"
        CarregarProdutos = False
        Exit Function
    End If

    sResp = oHttp.responseText
    Set oHttp = Nothing

    CarregarProdutos = ParsearJSON(sResp, produtos, total)
    Exit Function

ErrHandler:
    MsgBox "Erro ao chamar API: " & Err.Description, vbCritical, "Erro"
    CarregarProdutos = False
End Function

Private Function ParsearJSON(ByVal sJSON As String,
                              ByRef produtos() As Produto,
                              ByRef total As Long) As Boolean
    Dim i As Long
    Dim bloco As String
    Dim inicio As Long
    Dim fim As Long

    On Error GoTo ErrHandler

    sJSON = Trim(sJSON)

    total = 0
    Dim pos As Long
    pos = 1
    Do
        pos = InStr(pos, sJSON, """nome""")
        If pos = 0 Then Exit Do
        total = total + 1
        pos = pos + 6
    Loop

    If total = 0 Then
        ParsearJSON = False
        Exit Function
    End If

    ReDim produtos(0 To total - 1)

    pos = 1
    i = 0
    Do While i < total
        inicio = InStr(pos, sJSON, "{")
        fim = InStr(inicio, sJSON, "}")
        If inicio = 0 Or fim = 0 Then Exit Do

        bloco = Mid(sJSON, inicio, fim - inicio + 1)

        produtos(i).Nome = ExtrairValorString(bloco, "nome")
        produtos(i).Preco = ExtrairValorNumerico(bloco, "preco")

        pos = fim + 1
        i = i + 1
    Loop

    ParsearJSON = True
    Exit Function

ErrHandler:
    ParsearJSON = False
End Function

Private Function ExtrairValorString(ByVal sBloco As String,
                                     ByVal sChave As String) As String
    Dim p1 As Long, p2 As Long
    p1 = InStr(sBloco, """" & sChave & """")
    If p1 = 0 Then Exit Function

    p1 = InStr(p1, sBloco, ":") + 1
    Do While Mid(sBloco, p1, 1) = " " Or Mid(sBloco, p1, 1) = """"
        p1 = p1 + 1
        If Mid(sBloco, p1 - 1, 1) = """" Then Exit Do
    Loop
    p2 = InStr(p1, sBloco, """")
    ExtrairValorString = Mid(sBloco, p1, p2 - p1)
End Function

Private Function ExtrairValorNumerico(ByVal sBloco As String,
                                       ByVal sChave As String) As Double
    Dim p1 As Long, p2 As Long
    Dim sNum As String

    p1 = InStr(sBloco, """" & sChave & """")
    If p1 = 0 Then Exit Function

    p1 = InStr(p1, sBloco, ":") + 1

    Do While Mid(sBloco, p1, 1) = " "
        p1 = p1 + 1
    Loop
    p2 = p1
    Do While p2 <= Len(sBloco)
        Dim c As String
        c = Mid(sBloco, p2, 1)
        If c = "," Or c = "}" Or c = " " Then Exit Do
        p2 = p2 + 1
    Loop

    sNum = Mid(sBloco, p1, p2 - p1)

    sNum = Replace(sNum, ".", Application.DecimalSeparator)
    On Error Resume Next
    ExtrairValorNumerico = CDbl(sNum)
    On Error GoTo 0
End Function

Private Sub ExibirProdutos(ByRef produtos() As Produto, ByVal total As Long)
    Dim i As Long
    Dim oItem As ListItem

    ListView1.ListItems.Clear

    For i = 0 To total - 1
        Set oItem = ListView1.ListItems.Add(, , produtos(i).Nome)
        oItem.SubItems(1) = Format(produtos(i).Preco, "##,##0.00")
    Next i
End Sub
