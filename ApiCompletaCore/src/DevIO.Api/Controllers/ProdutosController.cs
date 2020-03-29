using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{
    [Route("api/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _IProdutoRepository;
        private readonly IProdutoService _IProdutoService;
        private readonly IMapper _IMapper;

        public ProdutosController(INotificador notificador, IProdutoRepository produtoRepository,
            IProdutoService produtoService, IMapper mapper) : base(notificador)
        {
            _IProdutoRepository = produtoRepository;
            _IProdutoService = produtoService;
            _IMapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _IMapper.Map<IEnumerable<ProdutoViewModel>>(await _IProdutoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();

            return produtoViewModel;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtoViewModel;

            if (!UploadArquivo(produtoViewModel.ImageUpload, imagemNome))
            {
                return CustomResponse(produtoViewModel);
            }

            produtoViewModel.Imagem = imagemNome;
            await _IProdutoService.Adicionar(_IMapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produto = await ObterProduto(id);

            if (produto == null) return NotFound();

            await _IProdutoService.Remover(id);

            return CustomResponse(produto);
        }


        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return  _IMapper.Map<ProdutoViewModel>(await _IProdutoRepository.ObterProdutoFornecedor(id));
        } 

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            var imateDateByteArray = Convert.FromBase64String(arquivo);

            if(string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma iamgem para este produto!");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome);

            if(System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imateDateByteArray);
            return true;
        }
    }
}
