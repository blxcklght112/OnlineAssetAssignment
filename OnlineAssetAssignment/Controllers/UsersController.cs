using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineAssetAssignment.Data;
using OnlineAssetAssignment.Models;

namespace OnlineAssetAssignment.Controllers
{
    public class UsersController : Controller
    {
        private readonly OaaContext _context;

        public UsersController(OaaContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
              return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            //Auto Generate Username
            string[] words = user.LastName.Split(' ');
            var subUsername = "";
            foreach(var word in words)
            {
                subUsername += word.Substring(0,1);
            }

            var _username = user.FirstName + subUsername;

            var _usernames = _context.Users.Select(x => x.Username).Where(y => y.Contains(_username)).ToArray();

            var oderedList = _usernames.OrderBy(x => new string(x.Where(char.IsLetter).ToArray()))
                .ThenBy(
                x =>
                {
                    int number;
                    if(int.TryParse(new string(x.Where(char.IsDigit).ToArray()), out number)) return number;
                return -1;
                });

            if(oderedList.Count() > 0) 
            {
                string a = oderedList.Last();
                string b = string.Empty;
                int c;
                
                for(int i=0; i < a?.Length; i++)
                {
                    if (Char.IsDigit(a[i]))
                    {
                        b += a[i];
                    }
                }

                if(b == "")
                {
                    _username += 1;
                }
                else if(b.Length > 0)
                {
                    a = a.Remove(a.Length - b.Length);
                    c = int.Parse(b) + 1;
                    _username += c.ToString();
                }
            }

            //Auto Generate Usercode
            var ids = _context.Users.Select(x => x.Id).ToArray();
            Array.Sort(ids);
            int subUserCode = ids.Last() + 1;

            var addingUser = await _context.Users.AddAsync(
                new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Dob= user.Dob,
                    JoinedDate = user.JoinedDate,
                    Gender = user.Gender,
                    Role = user.Role,
                    Username = _username,
                    Password = _username + "@" + user.Dob.ToString("ddMMyyyy"),
                    UserCode = $"SD{subUserCode:0000}",
                    IsFirstLogin = false
                });

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Dob,Gender,Role,JoinedDate,UserCode,Username,Password,IsFirstLogin")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'OaaContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return _context.Users.Any(e => e.Id == id);
        }
    }
}
