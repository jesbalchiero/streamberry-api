using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamberryApi.Data;
using StreamberryApi.Models;
using System.Globalization;

namespace StreamberryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FilmesController(AppDbContext context)
        {
            _context = context;
        }

        #region CRUD

        //GET: api/filmes
        [HttpGet]
        public ActionResult<IEnumerable<Filme>> GetFilmes()
        {
            return _context.Filmes.Include(f => f.Streamings)
                                   .Include(f => f.Generos)
                                   .Include(f => f.Avaliacoes)
                                   .ToList();
        }

        //GET: api/filmes/paginados
        [HttpGet("paginados")]
        public ActionResult<IEnumerable<Filme>> GetFilmesPaginados([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1)
                return BadRequest("O número da página deve ser maior ou igual a 1.");

            if (pageSize < 1)
                return BadRequest("O tamanho da página deve ser maior ou igual a 1.");

            var totalFilmes = _context.Filmes.Count();
            var totalPages = (int)Math.Ceiling((double)totalFilmes / pageSize);

            if (page > totalPages)
                return NotFound("Página não encontrada.");

            var filmes = _context.Filmes
                .Include(f => f.Streamings)
                .Include(f => f.Generos)
                .Include(f => f.Avaliacoes)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalFilmes = totalFilmes,
                TotalPages = totalPages,
                Filmes = filmes
            });
        }

        //GET: api/filmes/id
        [HttpGet("{id}")]
        public ActionResult<Filme> GetFilme(int id)
        {
            var filme = _context.Filmes.Include(f => f.Streamings)
                                       .Include(f => f.Generos)
                                       .Include(f => f.Avaliacoes)
                                       .FirstOrDefault(f => f.Id == id);

            if (filme == null)
                return BadRequest("Nenhum filme foi encontrado.");

            return filme;
        }

        //GET: api/filmes/pesquisar
        [HttpGet("pesquisar")]
        public IActionResult PesquisarFilmes([FromQuery] string titulo = null, [FromQuery] string dataLancamento = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var filmesQuery = _context.Filmes.Include(f => f.Streamings)
                                             .Include(f => f.Generos)
                                             .Include(f => f.Avaliacoes)
                                             .AsQueryable();

            if (!string.IsNullOrEmpty(titulo))
                filmesQuery = filmesQuery.Where(f => f.Titulo.Contains(titulo));
            
            if (!string.IsNullOrEmpty(dataLancamento))
            {
                if (DateTime.TryParseExact(dataLancamento, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    filmesQuery = filmesQuery.Where(f => f.DataLancamento.Date == parsedDate.Date);
                else
                    return BadRequest("Data de lançamento inválida. O formato deve ser dd/MM/yyyy.");
            }

            var totalFilmes = filmesQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalFilmes / pageSize);

            if (page < 1)
                page = 1;
            if (page > totalPages)
                page = totalPages;

            var filmes = filmesQuery.Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

            if (!filmes.Any())
                return NotFound("Nenhum filme encontrado com os critérios de pesquisa especificados.");

            return Ok(new
            {
                TotalFilmes = totalFilmes,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize,
                Filmes = filmes
            });
        }

        //POST: api/filmes
        [HttpPost]
        public IActionResult CreateFilme([FromBody] Filme filme)
        {
            _context.Filmes.Add(filme);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetFilme), new { id = filme.Id }, new { message = "Filme criado com sucesso.", filme });
        }

        //PUT: api/filmes/id
        [HttpPut("{id}")]
        public IActionResult UpdateFilme(int id, Filme filme)
        {
            if (id != filme.Id)
                return BadRequest("Nenhum filme foi encontrado para atualização.");

            _context.Entry(filme).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(new { message = "Filme atualizado com sucesso." });
        }

        //DELETE: api/filmes/id
        [HttpDelete("{id}")]
        public IActionResult DeleteFilme(int id)
        {
            var filme = _context.Filmes
                .Include(f => f.Avaliacoes)
                .Include(f => f.Generos)
                .Include(f => f.Streamings)
                .SingleOrDefault(f => f.Id == id);

            if (filme == null)
                return BadRequest("Nenhum filme foi encontrado para exclusão.");

            //foreign keys
            _context.Avaliacoes.RemoveRange(filme.Avaliacoes);
            _context.Generos.RemoveRange(filme.Generos);
            _context.Streamings.RemoveRange(filme.Streamings);

            _context.Filmes.Remove(filme);
            _context.SaveChanges();

            return Ok(new { message = "Filme excluído com sucesso." });
        }

        #endregion

        #region Em quantos Streamings um filme está disponível?

        //GET: /api/filmes/id/streamings
        [HttpGet("{id}/streamings")]
        public IActionResult GetStreamingsPorFilme(int id)
        {
            var filme = _context.Filmes
                .Include(f => f.Streamings)
                .FirstOrDefault(f => f.Id == id);

            if (filme == null)
                return NotFound("Filme não encontrado.");

            return Ok(new
            {
                TotalStreamings = filme.Streamings.Count,
                Streamings = filme.Streamings
            });
        }

        #endregion

        #region Qual a média de avaliação de cada filme?

        //GET: /api/filmes/media-avaliacao
        [HttpGet("media-avaliacao")]
        public IActionResult GetMediaAvaliacaoFilmes()
        {
            var filmesComMedia = _context.Filmes
                .Include(f => f.Avaliacoes)
                .Select(f => new
                {
                    FilmeId = f.Id,
                    Titulo = f.Titulo,
                    MediaAvaliacao = f.Avaliacoes.Any() ? f.Avaliacoes.Average(a => a.Pontuacao) : 0
                })
                .ToList();

            return Ok(filmesComMedia);
        }

        //GET: /api/filmes/{id}/media-avaliacao
        [HttpGet("{id}/media-avaliacao")]
        public IActionResult GetMediaAvaliacaoPorFilme(int id)
        {
            var filme = _context.Filmes
                .Include(f => f.Avaliacoes)
                .FirstOrDefault(f => f.Id == id);

            if (filme == null)
                return NotFound("Filme não encontrado.");

            var mediaAvaliacao = filme.Avaliacoes.Any() ? filme.Avaliacoes.Average(a => a.Pontuacao) : 0;

            return Ok(new
            {
                FilmeId = filme.Id,
                Titulo = filme.Titulo,
                MediaAvaliacao = mediaAvaliacao
            });
        }

        #endregion

        #region Quantos filmes e quais foram lançados em cada ano?

        //GET: /api/filmes/filmes-lancamento
        [HttpGet("filmes-lancamento")]
        public IActionResult GetFilmesPorLancamento()
        {
            var filmesPorAnoLancamento = _context.Filmes
                .GroupBy(f => f.DataLancamento.Year)
                .Select(g => new
                {
                    AnoLancamento = g.Key,
                    QuantidadeFilmes = g.Count(),
                    Filmes = g.Select(f => new { FilmeId = f.Id, Titulo = f.Titulo }).ToList()
                })
                .OrderByDescending(g => g.AnoLancamento)
                .ToList();

            return Ok(filmesPorAnoLancamento);
        }

        //GET: /api/filmes/{id}/lancamento
        [HttpGet("{id}/lancamento")]
        public IActionResult GetLancamentoFilme(int id)
        {
            var filme = _context.Filmes
                .Where(f => f.Id == id)
                .Select(f => new
                {
                    FilmeId = f.Id,
                    Titulo = f.Titulo,
                    AnoLancamento = f.DataLancamento.Year
                })
                .FirstOrDefault();

            if (filme == null)
                return NotFound("Filme não encontrado.");

            return Ok(filme);
        }


        #endregion

        #region Localizar filmes conforme avaliação e seus respectivos comentários
        
        //GET: /api/filmes/avaliacao
        //api/filmes/avaliacao?pontuacao=X
        //api/filmes/avaliacao?comentario=XXX
        //api/filmes/avaliacao?pontuacao=X&comentario=XXX
        [HttpGet("avaliacao")]
        public IActionResult GetFilmesPorAvaliacao([FromQuery] int? pontuacao, [FromQuery] string comentario = null)
        {
            if (!pontuacao.HasValue && string.IsNullOrEmpty(comentario))
                return BadRequest("Informe pelo menos uma pontuação ou um comentário para a consulta.");

            var filmesQuery = _context.Filmes.AsQueryable();

            if (pontuacao.HasValue || !string.IsNullOrEmpty(comentario))
                filmesQuery = filmesQuery.Where(f => f.Avaliacoes.Any(a =>
                    (!pontuacao.HasValue || a.Pontuacao == pontuacao.Value) &&
                    (string.IsNullOrEmpty(comentario) || a.Comentario.Contains(comentario))
                ));

            var filmes = filmesQuery.Select(f => new
            {
                FilmeId = f.Id,
                Titulo = f.Titulo,
                Avaliacoes = f.Avaliacoes
                    .Where(a => (!pontuacao.HasValue || a.Pontuacao == pontuacao.Value) &&
                                (string.IsNullOrEmpty(comentario) || a.Comentario.Contains(comentario)))
            }).ToList();

            if (!filmes.Any())
                return NotFound("Nenhum filme encontrado com os critérios de consulta especificados.");

            return Ok(filmes);
        }

        #endregion

        #region Quais são as avaliações médias de filmes agrupados por gênero conforme a época de lançamento

        // GET: api/filmes/media-avaliacoes-genero
        [HttpGet("media-avaliacoes-genero")]
        public IActionResult GetMediaAvaliacoesPorLancamentoGenero()
        {
            var mediaAvaliacoesPorDataEGenero = _context.Filmes
                .SelectMany(f => f.Generos, (filme, genero) => new { Filme = filme, Genero = genero })
                .GroupBy(
                    fg => new { AnoLancamento = fg.Filme.DataLancamento.Year, Genero = fg.Genero.Nome },
                    (key, filmesPorDataEGenero) => new
                    {
                        AnoLancamento = key.AnoLancamento,
                        Genero = key.Genero,
                        MediaAvaliacao = filmesPorDataEGenero.Average(f => f.Filme.Avaliacoes.Any() ? f.Filme.Avaliacoes.Average(a => a.Pontuacao) : 0)
                    }
                )
                .OrderBy(r => r.AnoLancamento)
                .ThenBy(r => r.Genero)
                .ToList();

            return Ok(mediaAvaliacoesPorDataEGenero);
        }

        #endregion
    }
}
