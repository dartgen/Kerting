<template>
  <div class="flex items-center justify-center w-full min-h-[calc(100dvh-5rem)] p-4 sm:p-6 bg-earth-950 relative">

    <div class="relative w-full max-w-[1400px] h-[85vh] flex bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden animate-fade-in">

      <div v-if="isLoading" class="absolute inset-0 z-50 flex flex-col items-center justify-center bg-earth-900/90 backdrop-blur-sm">
        <svg class="w-12 h-12 text-green-500 animate-spin mb-4" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <p class="text-earth-300 font-bold tracking-widest uppercase animate-pulse">Projektek betöltése...</p>
      </div>

      <div class="w-full md:w-80 lg:w-96 flex-shrink-0 border-r border-earth-100/20 flex flex-col bg-earth-900/40"
           :class="{'hidden md:flex': selectedProjectId !== null}">

        <div class="p-5 border-b border-earth-100/10">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-bold text-earth-50 tracking-wide">Projektek</h2>
            <button @click="ujProjektKezdese" class="bg-green-600 hover:bg-green-500 text-white w-8 h-8 rounded-lg flex items-center justify-center transition-all shadow-md" title="Új projekt létrehozása">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4"/></svg>
            </button>
          </div>

          <div class="flex bg-earth-950/50 rounded-lg p-1 border border-earth-100/10 gap-1">
            <button @click="activeFilter = 'ongoing'"
                    :class="['flex-1 py-1.5 text-xs sm:text-sm font-semibold rounded-md transition-all border', activeFilter === 'ongoing' ? 'bg-green-500/20 text-green-400 border-green-500/30 shadow-sm' : 'border-transparent text-earth-400 hover:text-earth-200 hover:bg-earth-800/30']">
              Aktív
            </button>
            <button @click="activeFilter = 'invited'"
                    :class="['flex-1 py-1.5 text-xs sm:text-sm font-semibold rounded-md transition-all flex items-center justify-center gap-1.5 border', activeFilter === 'invited' ? 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30 shadow-sm' : 'border-transparent text-earth-400 hover:text-earth-200 hover:bg-earth-800/30']">
              Meghívók
              <span v-if="inviteCount > 0" :class="activeFilter === 'invited' ? 'bg-yellow-400 text-yellow-950' : 'bg-earth-700 text-earth-300'" class="text-[9px] px-1.5 py-0.5 rounded-full leading-none font-bold transition-colors">{{ inviteCount }}</span>
            </button>
            <button @click="activeFilter = 'archived'"
                    :class="['flex-1 py-1.5 text-xs sm:text-sm font-semibold rounded-md transition-all border', activeFilter === 'archived' ? 'bg-earth-800/80 text-earth-300 border-earth-600 shadow-sm' : 'border-transparent text-earth-400 hover:text-earth-200 hover:bg-earth-800/30']">
              Archivált
            </button>
          </div>
        </div>

        <div class="flex-1 overflow-y-auto custom-scrollbar p-3 space-y-2">
          <div v-if="filteredProjects.length === 0 && !isLoading" class="text-center text-earth-400 py-6 text-sm">
            <span v-if="activeFilter === 'invited'">Nincs új meghívód.</span>
            <span v-else>Nincs megjeleníthető projekt.</span>
          </div>

          <div v-for="project in filteredProjects" :key="project.id"
               @click="selectedProjectId = project.id"
               :class="[
                 'p-4 rounded-xl cursor-pointer transition-all border-l-4 group relative overflow-hidden',
                 selectedProjectId === project.id
                  ? (project.status === 'invited' ? 'bg-yellow-900/10 border-yellow-500 shadow-md' :
                     project.status === 'archived' ? 'bg-earth-800/50 border-earth-500 shadow-md' :
                     'bg-green-900/10 border-green-500 shadow-md')
                  : 'bg-earth-900/20 border-transparent hover:bg-earth-800/40 hover:border-earth-500/50'
               ]">
            <div v-if="project.status === 'invited'" class="absolute top-0 right-0 w-12 h-12 bg-yellow-500/10 -translate-y-6 translate-x-6 rotate-45"></div>

            <h3 class="text-earth-50 font-bold truncate text-base relative z-10">{{ project.title }}</h3>
            <p class="text-xs text-earth-300 mt-1 line-clamp-1 relative z-10">{{ project.description }}</p>
            <div class="flex justify-between items-center mt-3 relative z-10">
              <span class="text-[10px] text-earth-400 font-semibold uppercase flex items-center gap-1">
                Határidő: {{ project.deadline || 'Nincs' }}
              </span>
              <span v-if="project.status === 'archived'" class="text-[10px] bg-earth-800 border border-earth-700 text-earth-400 px-2 py-0.5 rounded-full font-bold uppercase shadow-inner">Archivált</span>
              <span v-else-if="project.status === 'invited'" class="text-[10px] bg-yellow-500/20 border border-yellow-500/30 text-yellow-400 px-2 py-0.5 rounded-full font-bold uppercase shadow-inner">Új meghívó</span>
              <span v-else class="text-[10px] bg-green-500/20 border border-green-500/30 text-green-400 px-2 py-0.5 rounded-full font-bold uppercase shadow-inner">Aktív</span>
            </div>
          </div>
        </div>
      </div>

      <div class="flex-1 flex flex-col min-w-0 overflow-y-auto custom-scrollbar" :class="{'hidden md:flex': selectedProjectId === null}">

        <template v-if="activeProject && !isLoading">
          <div class="p-6 md:p-8 flex flex-col gap-6">

            <button @click="selectedProjectId = null" class="md:hidden flex items-center gap-2 text-earth-300 hover:text-earth-50 transition-colors w-fit mb-[-10px]">
              Vissza a listához
            </button>

            <div v-if="activeProject.status === 'invited'" class="bg-gradient-to-r from-earth-800/80 to-yellow-900/20 border border-yellow-500/30 rounded-2xl p-5 flex flex-col sm:flex-row justify-between items-center gap-4 shadow-lg animate-fade-in">
              <div class="flex items-center gap-4">
                <div class="w-12 h-12 bg-yellow-500/20 rounded-full flex items-center justify-center text-yellow-400 shrink-0">
                  <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 19v-8.93a2 2 0 01.89-1.664l7-4.666a2 2 0 012.22 0l7 4.666A2 2 0 0121 10.07V19M3 19a2 2 0 002 2h14a2 2 0 002-2M3 19l6.75-4.5M21 19l-6.75-4.5M3 10l6.75 4.5M21 10l-6.75 4.5m0 0l-1.14.76a2 2 0 01-2.22 0l-1.14-.76"/></svg>
                </div>
                <div>
                  <h3 class="text-earth-50 font-bold text-lg">Meghívó ebbe a projektbe</h3>
                  <p class="text-earth-300 text-sm">Meghívtak, hogy vegyél részt ebben a munkában. Elfogadod?</p>
                </div>
              </div>
              <div class="flex gap-3 w-full sm:w-auto">
                <button @click="elutasitMeghivo" class="flex-1 sm:flex-none px-4 py-2 bg-earth-800 hover:bg-red-900/40 text-earth-300 hover:text-red-400 border border-earth-700 hover:border-red-500/50 rounded-xl transition-all text-sm font-bold">
                  Elutasít
                </button>
                <button @click="elfogadMeghivo" class="flex-1 sm:flex-none px-6 py-2 bg-green-600 hover:bg-green-500 text-white rounded-xl transition-all shadow-md active:scale-95 text-sm font-bold">
                  Elfogadom
                </button>
              </div>
            </div>

            <div class="bg-earth-800/40 border border-earth-100/10 rounded-2xl p-6 flex flex-col xl:flex-row justify-between items-start xl:items-center gap-4 relative">

              <div v-if="isProjectOwner && activeProject.status !== 'invited'" class="absolute top-4 right-4 flex gap-2">
                <button @click="projektSzerkesztese" class="bg-earth-800 text-earth-300 hover:text-green-400 border border-earth-700 hover:border-green-500/30 w-8 h-8 rounded-lg flex items-center justify-center transition-all shadow-sm" title="Projekt szerkesztése">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/></svg>
                </button>

                <button v-if="activeProject.status !== 'archived'" @click="projektArchivalasa(true)" class="bg-earth-800 text-earth-400 hover:text-yellow-400 border border-earth-700 hover:border-yellow-500/30 w-8 h-8 rounded-lg flex items-center justify-center transition-all shadow-sm" title="Projekt archiválása">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"/></svg>
                </button>
                <button v-else @click="projektArchivalasa(false)" class="bg-earth-800 text-earth-400 hover:text-green-400 border border-earth-700 hover:border-green-500/30 w-8 h-8 rounded-lg flex items-center justify-center transition-all shadow-sm" title="Projekt aktiválása">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>
                </button>

                <button @click="projektTorlese" class="bg-earth-800 text-earth-400 hover:text-red-400 border border-earth-700 hover:border-red-500/30 w-8 h-8 rounded-lg flex items-center justify-center transition-all shadow-sm" title="Projekt törlése">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                </button>
              </div>

              <div v-if="!isProjectOwner && activeProject.status !== 'invited'" class="absolute top-4 right-4 flex items-center gap-2 bg-earth-900/80 px-3 py-1.5 rounded-lg border border-earth-100/10">
                <svg class="w-3.5 h-3.5 text-earth-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>
                <span class="text-xs font-semibold text-earth-300">Tulajdonos: {{ getMemberName(activeProject.ownerId) }}</span>
              </div>

              <div class="pr-32 w-full">
                <h1 class="text-2xl sm:text-3xl font-bold text-earth-50 leading-tight">{{ activeProject.title }}</h1>
                <p class="text-earth-300 text-sm mt-1.5">{{ activeProject.description }}</p>
              </div>

              <div class="flex flex-col items-end shrink-0 mt-2 xl:mt-0 xl:pt-8 w-full xl:w-auto">
                <div class="flex items-center gap-2 text-orange-400 font-bold bg-orange-400/10 px-4 py-2 rounded-xl border border-orange-400/20 shadow-sm w-full xl:w-auto justify-center">
                  <span>Határidő: {{ activeProject.deadline || 'Nincs megadva' }}</span>
                </div>
              </div>
            </div>

            <div v-if="isProjectOwner" class="grid grid-cols-1 xl:grid-cols-3 gap-6 animate-fade-in">

              <div class="xl:col-span-1 flex flex-col gap-6">
                <div class="bg-earth-800/40 border border-earth-100/10 rounded-2xl p-5 shadow-sm">

                  <div class="flex justify-between items-start mb-4">
                    <div>
                      <h3 class="text-earth-50 font-bold">Csapat</h3>
                      <span class="text-[9px] text-earth-400 uppercase font-bold tracking-wider block mt-1">Húzd a feladatra</span>
                    </div>
                    <button @click="showUserSelector = true" class="text-[10px] bg-green-600 hover:bg-green-500 text-white px-2 py-1.5 rounded-lg transition-all shadow-md flex items-center gap-1 font-bold uppercase tracking-wide">
                      <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4"/></svg>
                      Új tag
                    </button>
                  </div>

                  <div class="space-y-3">
                    <div v-for="member in activeProject.members" :key="member.userId"
                         draggable="true"
                         @dragstart="startDrag($event, member)"
                         @dragend="endDrag"
                         class="flex items-center gap-3 p-3 bg-earth-900/50 rounded-xl border border-earth-100/5 cursor-grab active:cursor-grabbing hover:border-green-500/50 hover:bg-earth-800/80 transition-all shadow-sm group">

                      <div class="text-earth-600 group-hover:text-earth-400 px-1 opacity-50 group-hover:opacity-100 transition-opacity">
                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 9h2v2H8V9zm0 4h2v2H8v-2zm6-4h2v2h-2V9zm0 4h2v2h-2v-2z"/></svg>
                      </div>

                      <div @click="mutasdTagReszleteit(member)" class="flex flex-1 items-center gap-3 cursor-pointer hover:bg-earth-800/50 rounded-lg transition-colors p-1 -m-1">

                        <div class="w-10 h-10 rounded-full bg-earth-800 border border-earth-100/20 overflow-hidden shrink-0 flex items-center justify-center text-earth-400">
                          <img v-if="member.avatar"
                               :src="getImageUrl(member.avatar)"
                               @error="hideImage"
                               class="w-full h-full object-cover">
                          <svg v-else class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" /></svg>
                        </div>

                        <div class="flex-1 min-w-0">
                          <p class="text-earth-50 text-sm font-semibold truncate group-hover:text-green-400 transition-colors">
                            {{ getMemberName(member.userId) }}
                            <span v-if="isMe(member.userId)" class="text-[9px] bg-green-500/20 text-green-400 px-1.5 py-0.5 rounded ml-1 uppercase">Tulaj</span>
                          </p>
                          <p class="text-earth-400 text-[10px] uppercase tracking-wider truncate">{{ member.role }}</p>
                        </div>

                      </div>

                      <button v-if="!isMe(member.userId)" @click.stop="tagEltavolitasa(member.userId)" class="text-earth-500 hover:text-red-400 transition-colors shrink-0 z-10" title="Eltávolítás">
                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <div class="xl:col-span-2 flex flex-col gap-6">
                <div class="bg-earth-800/40 border border-earth-100/10 rounded-2xl p-5 shadow-sm">

                  <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
                    <h3 class="text-earth-50 font-bold">Feladatok Kezelése</h3>
                    <button @click="ujFeladatKezdese" class="bg-green-600 hover:bg-green-500 text-white font-bold py-2 px-4 text-sm rounded-xl transition-all shadow-md active:scale-95 flex items-center gap-2">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
                      Új feladat
                    </button>
                  </div>

                  <div class="space-y-4">
                    <div v-for="task in activeProject.tasks" :key="task.id" class="relative group">

                      <div class="bg-earth-900/50 border border-earth-700/50 p-4 rounded-xl transition-all shadow-sm h-full"
                           :class="{'opacity-50 blur-[1px]': isDragging && dragHoverTaskId !== task.id}">

                        <div class="absolute top-3 right-3 flex gap-2 z-10">
                          <button @click="feladatSzerkesztese(task)" class="bg-earth-800 text-earth-400 hover:text-green-400 border border-earth-700 hover:border-green-500/30 w-7 h-7 rounded flex items-center justify-center transition-colors shadow-sm" title="Szerkesztés">
                            <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/></svg>
                          </button>
                          <button @click="feladatTorlese(task.id)" class="bg-earth-800 text-earth-500 hover:text-red-400 border border-earth-700 hover:border-red-500/30 w-7 h-7 rounded flex items-center justify-center transition-colors shadow-sm" title="Törlés">
                            <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                          </button>
                        </div>

                        <div class="flex flex-col xl:flex-row justify-between gap-4 pr-20">
                          <div class="flex-1">
                            <h4 @click="mutasdFeladatReszleteit(task)" class="text-earth-50 font-bold text-base sm:text-lg hover:text-green-400 cursor-pointer transition-colors flex items-center gap-2 group/title">
                              {{ task.title }}
                              <svg class="w-3.5 h-3.5 opacity-0 group-hover/title:opacity-100 transition-opacity" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"/></svg>
                            </h4>
                            <p v-if="task.description" class="text-earth-300 text-xs mt-1">{{ task.description }}</p>

                            <div class="flex flex-wrap items-center gap-3 mt-3">
                              <span class="text-[10px] text-earth-400 font-medium bg-earth-950 px-2.5 py-1.5 rounded-md border border-earth-800 flex items-center shadow-inner">
                                Határidő: {{ task.deadline || 'Nincs' }}
                              </span>
                              <span v-if="task.amount" class="text-[10px] text-earth-400 font-medium bg-earth-950 px-2.5 py-1.5 rounded-md border border-earth-800 flex items-center shadow-inner text-green-400">
                                {{ formatCurrency(task.amount) }}
                              </span>

                              <div class="relative flex items-center ml-auto xl:ml-0 shrink-0">
                                <select v-model="task.status" @change="feladatMentese(task)" :class="statusClass(task.status)" class="text-[10px] pl-3 pr-7 py-1.5 rounded-md font-bold uppercase border-none outline-none cursor-pointer appearance-none shadow-sm transition-all focus:ring-2 focus:ring-green-500/30">
                                  <option value="todo" class="bg-earth-900 text-white font-bold uppercase">Tennivaló</option>
                                  <option value="in-progress" class="bg-earth-900 text-white font-bold uppercase">Folyamatban</option>
                                  <option value="done" class="bg-earth-900 text-white font-bold uppercase">Kész</option>
                                </select>
                                <svg class="w-3 h-3 absolute right-2 pointer-events-none" :class="statusIconClass(task.status)" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7"/></svg>
                              </div>

                            </div>
                          </div>

                          <div class="flex flex-col gap-1.5 min-w-[140px] xl:border-l xl:border-earth-100/10 xl:pl-4">
                            <label class="text-[10px] text-earth-400 uppercase font-bold">Felelősök:</label>
                            <div class="flex flex-wrap gap-1.5">
                              <template v-if="task.assignedTo && task.assignedTo.length > 0">
                                <div v-for="userId in task.assignedTo" :key="userId"
                                     @click.stop="megnyitProfil(userId)"
                                     :class="['text-[10px] pl-2 pr-1 py-1 rounded flex items-center gap-1 shadow-sm border cursor-pointer hover:opacity-80 transition-opacity', isMe(userId) ? 'bg-green-900/30 border-green-500/50 text-green-400' : 'bg-earth-800 border-earth-600 text-earth-100']">
                                  <div class="w-3.5 h-3.5 rounded-full overflow-hidden mr-1 bg-earth-950 flex-shrink-0">
                                    <img v-if="getMemberDetails(userId)?.avatar"
                                         :src="getImageUrl(getMemberDetails(userId)?.avatar)"
                                         @error="hideImage"
                                         class="w-full h-full object-cover">
                                    <svg v-else class="w-3 h-3 text-earth-400 m-auto mt-0.5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" /></svg>
                                  </div>
                                  <span>{{ getMemberName(userId) }}</span>

                                  <button @click.stop="feladatbolEltavolit(task, userId)" class="ml-0.5 p-0.5 text-earth-500 hover:text-red-400 hover:bg-red-400/10 rounded-full transition-all" title="Eltávolítás a feladatból">
                                    <svg class="w-2.5 h-2.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                                  </button>

                                </div>
                              </template>
                              <span v-else class="text-xs text-earth-500 italic">Nincs kiosztva</span>
                            </div>
                          </div>
                        </div>
                      </div>

                      <div v-if="isDragging"
                           @dragover.prevent="dragHoverTaskId = task.id"
                           @dragleave.prevent="dragHoverTaskId = null"
                           @drop="onDrop($event, task)"
                           class="absolute inset-0 z-20 rounded-xl transition-all border-2 flex items-center justify-center backdrop-blur-sm"
                           :class="dragHoverTaskId === task.id ? 'border-green-500 bg-green-500/20' : 'border-transparent bg-earth-950/40'">

                        <div v-if="dragHoverTaskId === task.id" class="bg-green-600 text-white px-4 py-2 rounded-xl font-bold shadow-2xl flex items-center gap-2 pointer-events-none animate-fade-in scale-110 transition-transform">
                          <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"/></svg>
                          Hozzáadás: {{ draggedMember?.name }}
                        </div>
                      </div>

                    </div>

                    <div v-if="activeProject.tasks.length === 0" class="text-center py-10 text-earth-400 border-2 border-dashed border-earth-700/50 rounded-xl bg-earth-900/20">
                      Nincsenek feladatok. Hozz létre egyet!
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div v-else class="flex flex-col gap-6 animate-fade-in">

              <div class="bg-earth-800/40 border border-green-500/20 rounded-2xl p-6 shadow-sm flex flex-col sm:flex-row justify-between items-center gap-4">
                <div>
                  <h3 class="text-earth-50 font-bold flex items-center gap-2">
                    <svg class="w-5 h-5 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                    Saját várható kereseted
                  </h3>
                  <p class="text-earth-400 text-xs mt-1">Ez az összeg a rád kiosztott feladatokból számolódik (megosztott feladatnál arányosan).</p>
                </div>
                <div class="text-3xl font-bold text-green-400 bg-green-500/10 px-6 py-3 rounded-xl border border-green-500/30 shadow-inner">
                  {{ formatCurrency(myExpectedEarnings) }}
                </div>
              </div>

              <div class="bg-earth-800/40 border border-earth-100/10 rounded-2xl p-5 shadow-sm">
                <div class="mb-6 border-b border-earth-100/10 pb-4">
                  <h3 class="text-earth-50 font-bold text-lg">Rám kiosztott feladatok ({{ myTasks.length }} db)</h3>
                </div>

                <div class="space-y-4">
                  <div v-for="task in myTasks" :key="task.id" class="bg-earth-900/50 border border-earth-700/50 p-5 rounded-xl shadow-sm">
                    <div class="flex flex-col md:flex-row justify-between gap-6">

                      <div class="flex-1">
                        <h4 @click="mutasdFeladatReszleteit(task)" class="text-earth-50 font-bold text-lg hover:text-green-400 cursor-pointer transition-colors flex items-center gap-2 group/title">
                          {{ task.title }}
                          <svg class="w-4 h-4 opacity-0 group-hover/title:opacity-100 transition-opacity" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"/></svg>
                        </h4>
                        <p v-if="task.description" class="text-earth-300 text-sm mt-2">{{ task.description }}</p>

                        <div class="flex flex-wrap items-center gap-3 mt-4">
                          <span class="text-xs text-earth-400 font-medium bg-earth-950 px-3 py-1.5 rounded-md border border-earth-800">
                            Határidő: {{ task.deadline || 'Nincs' }}
                          </span>
                          <span v-if="task.amount" class="text-xs text-earth-400 font-medium bg-earth-950 px-3 py-1.5 rounded-md border border-earth-800 text-green-400">
                            Teljes díj: {{ formatCurrency(task.amount) }}
                          </span>
                        </div>
                      </div>

                      <div class="flex flex-col sm:flex-row md:flex-col items-start md:items-end justify-between shrink-0 gap-4 md:border-l md:border-earth-100/10 md:pl-6 min-w-[200px]">

                        <div class="flex flex-col gap-2 w-full md:items-end">
                          <label class="text-[10px] text-earth-400 uppercase font-bold">Kivel dolgozol ezen:</label>
                          <div class="flex flex-wrap gap-2 justify-start md:justify-end">
                            <div v-for="userId in task.assignedTo" :key="userId"
                                 @click.stop="megnyitProfil(userId)"
                                 :title="getMemberName(userId)"
                                 :class="['w-9 h-9 rounded-full overflow-hidden border-2 flex items-center justify-center shrink-0 shadow-sm transition-transform hover:scale-110 cursor-pointer', isMe(userId) ? 'border-green-500 bg-green-900/30 text-green-400' : 'border-earth-600 bg-earth-800 text-earth-400']">
                              <img v-if="getMemberDetails(userId)?.avatar"
                                   :src="getImageUrl(getMemberDetails(userId)?.avatar)"
                                   @error="hideImage"
                                   class="w-full h-full object-cover">
                              <svg v-else class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" /></svg>
                            </div>
                          </div>
                        </div>

                        <div class="flex flex-col w-full md:items-end mt-auto">
                          <label class="text-[10px] text-earth-400 uppercase font-bold mb-1">Állapot jelentése:</label>
                          <div class="relative w-full md:w-auto">
                            <select v-model="task.status" @change="feladatMentese(task)" :class="statusClass(task.status)" class="text-xs pl-4 pr-10 py-2 rounded-lg font-bold uppercase border-none outline-none cursor-pointer appearance-none shadow-sm text-center w-full transition-all focus:ring-2 focus:ring-green-500/30">
                              <option value="todo" class="bg-earth-900 text-white font-bold uppercase">Tennivaló</option>
                              <option value="in-progress" class="bg-earth-900 text-white font-bold uppercase">Folyamatban</option>
                              <option value="done" class="bg-earth-900 text-white font-bold uppercase">Kész vagyok vele</option>
                            </select>
                            <svg class="w-4 h-4 absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none" :class="statusIconClass(task.status)" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7"/></svg>
                          </div>
                        </div>

                      </div>
                    </div>
                  </div>

                  <div v-if="myTasks.length === 0" class="text-center py-10 text-earth-400 border-2 border-dashed border-earth-700/50 rounded-xl bg-earth-900/20">
                    Még nem osztottak ki neked feladatot ebben a projektben.
                  </div>
                </div>
              </div>
            </div>

          </div>
        </template>

        <div v-else-if="!isLoading" class="flex-1 flex flex-col items-center justify-center text-earth-300/50 p-10 text-center">
          <h3 class="text-xl font-semibold text-earth-50/50 tracking-wide">Nincs projekt kiválasztva</h3>
          <p class="mt-2 max-w-sm text-sm">Válassz egyet a bal oldali listából a részletek megtekintéséhez, vagy hozz létre egy újat.</p>
        </div>

      </div>
    </div>

    <UserSelectorModal
      v-if="showUserSelector"
      @close="showUserSelector = false"
      @select="felhasznaloHozzaadasa"
    />
    <ProjectEditModal
      v-if="showProjectModal"
      :editData="kivalasztottProjektSzerkesztesre"
      @close="showProjectModal = false"
      @save="projektMentese"
    />
    <TaskCreateModal
      v-if="showTaskCreateModal"
      :projectMembers="activeProject?.members || []"
      :defaultDeadline="activeProject?.deadline"
      :editData="kivalasztottFeladatSzerkesztesre"
      @close="showTaskCreateModal = false"
      @save="feladatMentese"
    />
    <MemberDetailsModal
      v-if="selectedMemberForDetails"
      :member="selectedMemberForDetails"
      :projectTasks="activeProject?.tasks || []"
      @close="selectedMemberForDetails = null"
    />
    <TaskDetailModal
      v-if="kivalasztottFeladatReszletek"
      :task="kivalasztottFeladatReszletek"
      :projectMembers="activeProject?.members || []"
      :currentUserId="currentUserId"
      @close="kivalasztottFeladatReszletek = null"
      @save-task="feladatMentese"
    />

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { projectService } from '@/services/projectService';
import apiClient from '@/services/axios';
import type { Project, Task, ProjectMember } from '@/types/project';

import ProjectEditModal from '@/components/project/ProjectEditModal.vue';
import TaskCreateModal from '@/components/project/TaskCreateModal.vue';
import MemberDetailsModal from '@/components/project/MemberDetailsModal.vue';
import TaskDetailModal from '@/components/project/TaskDetailModal.vue';
import UserSelectorModal from '@/components/project/UserSelectorModal.vue';

// --- Biztonságos kép eltüntetés hibás betöltés esetén ---
const hideImage = (event: Event) => {
  const target = event.target as HTMLElement;
  if (target) {
    target.style.display = 'none';
  }
};

// --- AUTHENTIKÁCIÓ ÉS AZONOSÍTÁS ---
const authStore = useAuthStore();

// Megjegyzés: A (authStore.profilAdatok as any) azért kell, hogy a TS ne sírjon a hiányzó tulajdonságok miatt a build során
const currentUserId = computed(() => String((authStore.profilAdatok as any)?.id || '').toLowerCase().trim());
const currentUserName = computed(() => String((authStore.profilAdatok as any)?.felhasznalonev || (authStore.profilAdatok as any)?.username || '').toLowerCase().trim());

const isMe = (userId: string) => {
  if (!userId) return false;
  const uId = String(userId).toLowerCase().trim();
  return uId === currentUserId.value || uId === currentUserName.value;
};

// --- ÁLLAPOTOK ---
const activeFilter = ref<'ongoing' | 'archived' | 'invited'>('ongoing');
const selectedProjectId = ref<number | null>(null);
const isLoading = ref(true);

const showProjectModal = ref(false);
const showTaskCreateModal = ref(false);
const showUserSelector = ref(false);

const kivalasztottProjektSzerkesztesre = ref<any>(null);
const kivalasztottFeladatSzerkesztesre = ref<any>(null);
const selectedMemberForDetails = ref<any>(null);
const kivalasztottFeladatReszletek = ref<any>(null);

const isDragging = ref(false);
const draggedMember = ref<ProjectMember | null>(null);
const dragHoverTaskId = ref<number | null>(null);

const projectsList = ref<Project[]>([]);

// --- ÚJ URL GENERÁLÓ (Képekhez) ---
// Típus javítva: string | undefined
const getImageUrl = (fileName: string | null | undefined): string | undefined => {
  if (!fileName || fileName.trim() === '') return undefined;
  if (fileName.startsWith('http')) return fileName;
  const axiosBaseUrl = apiClient.defaults.baseURL;
  if (!axiosBaseUrl) return undefined;
  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`;
};

// --- BETÖLTÉS ---
const loadProjects = async () => {
  try {
    isLoading.value = true;
    projectsList.value = await projectService.getMyProjects();

    if (filteredProjects.value.length > 0 && !selectedProjectId.value) {
      const firstProject = filteredProjects.value[0];
      if (firstProject) {
        selectedProjectId.value = firstProject.id;
      }
    }
  } catch (error) {
    console.error("Hiba a projektek betöltésekor:", error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  if (currentUserId.value || currentUserName.value) {
    loadProjects();
  }
});

const filteredProjects = computed(() => projectsList.value.filter(p => p.status === activeFilter.value));
const activeProject = computed(() => projectsList.value.find(p => p.id === selectedProjectId.value) || null);

const isProjectOwner = computed(() => {
  if (!activeProject.value || !activeProject.value.ownerId) return false;
  return isMe(activeProject.value.ownerId);
});

const myTasks = computed(() => {
  if (!activeProject.value) return [];
  return activeProject.value.tasks.filter(task =>
    task.assignedTo && task.assignedTo.some(id => isMe(id))
  );
});

const myExpectedEarnings = computed(() => {
  let sum = 0;
  myTasks.value.forEach(task => {
    if (task.amount && task.assignedTo.length > 0) {
      sum += (task.amount / task.assignedTo.length);
    }
  });
  return sum;
});

const inviteCount = computed(() => projectsList.value.filter(p => p.status === 'invited').length);

const statusClass = (status: string) => {
  if (status === 'done') return 'bg-green-500/20 text-green-400 border border-green-500/30';
  if (status === 'in-progress') return 'bg-blue-500/20 text-blue-400 border border-blue-500/30';
  return 'bg-earth-800 text-earth-300 border border-earth-600';
};

const statusIconClass = (status: string) => {
  if (status === 'done') return 'text-green-400';
  if (status === 'in-progress') return 'text-blue-400';
  return 'text-earth-400';
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(amount);
};

const getMemberDetails = (userId: string) => {
  if (!activeProject.value) return null;
  return activeProject.value.members.find(m => String(m.userId).toLowerCase() === String(userId).toLowerCase()) || null;
};

const getMemberName = (userId: string) => {
  if (!userId) return 'Ismeretlen';

  // Javítva as any kényszerítéssel a TS miatt
  const userNameToDisplay = (authStore.profilAdatok as any)?.nev || (authStore.profilAdatok as any)?.felhasznalonev || (authStore.profilAdatok as any)?.username || 'Én';

  if (isMe(userId)) return userNameToDisplay;

  const member = getMemberDetails(userId);
  if (member && member.name === 'Én') return userNameToDisplay;

  return member ? member.name : 'Ismeretlen';
};

// --- DRAG AND DROP ---
const startDrag = (event: DragEvent, member: ProjectMember) => {
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'copy';
    event.dataTransfer.setData('text/plain', member.userId);
  }
  isDragging.value = true;
  draggedMember.value = member;
};

const endDrag = () => {
  isDragging.value = false;
  draggedMember.value = null;
  dragHoverTaskId.value = null;
};

const onDrop = async (event: DragEvent, task: Task) => {
  dragHoverTaskId.value = null;
  const userId = draggedMember.value?.userId;
  if (!userId || !activeProject.value) return;

  if (!task.assignedTo) task.assignedTo = [];

  if (!task.assignedTo.includes(userId)) {
    task.assignedTo.push(userId);
    try {
      await projectService.saveTask(activeProject.value.id, task);
    } catch (error) {
      console.error("Nem sikerült elmenteni a hozzárendelést", error);
      task.assignedTo = task.assignedTo.filter(id => id !== userId);
    }
  }
};

const feladatbolEltavolit = async (task: Task, userIdToRemove: string) => {
  if (!task.assignedTo) return;
  task.assignedTo = task.assignedTo.filter(id => String(id) !== String(userIdToRemove));
  await feladatMentese(task);
};

const mutasdFeladatReszleteit = (task: Task) => {
  kivalasztottFeladatReszletek.value = task;
};

const elfogadMeghivo = async () => {
  if (!activeProject.value) return;
  try {
    await projectService.acceptInvite(activeProject.value.id);
    const index = projectsList.value.findIndex(p => p.id === activeProject.value!.id);
    // Biztonsági null check indexhez
    if (index !== -1 && projectsList.value[index]) {
      projectsList.value[index].status = 'ongoing';
      activeFilter.value = 'ongoing';
    }
  } catch (error) {
    console.error("Hiba a meghívó elfogadásakor", error);
  }
};

const elutasitMeghivo = async () => {
  if (!activeProject.value) return;
  try {
    await projectService.rejectInvite(activeProject.value.id);
    projectsList.value = projectsList.value.filter(p => p.id !== activeProject.value!.id);
    selectedProjectId.value = null;
  } catch (error) {
    console.error("Hiba a meghívó elutasításakor", error);
  }
};

const ujProjektKezdese = () => {
  kivalasztottProjektSzerkesztesre.value = null;
  showProjectModal.value = true;
};

const projektSzerkesztese = () => {
  if (!activeProject.value) return;
  kivalasztottProjektSzerkesztesre.value = {
    ...activeProject.value,
    memberNamesRaw: activeProject.value.members.map(m => m.name).join(', ')
  };
  showProjectModal.value = true;
};

const projektMentese = async (projectData: any) => {
  try {
    if (projectData.id) {
      const updatedProject = await projectService.updateProject(projectData.id, projectData);
      const index = projectsList.value.findIndex(p => p.id === updatedProject.id);
      if (index !== -1) projectsList.value[index] = updatedProject;
    } else {
      const newProject = await projectService.createProject(projectData);
      projectsList.value.push(newProject);
      selectedProjectId.value = newProject.id;
      activeFilter.value = newProject.status;
    }
  } catch (error) {
    console.error("Hiba a projekt mentésekor", error);
  }
};

const projektArchivalasa = async (isArchiving: boolean) => {
  if (!activeProject.value) return;
  const confirmMsg = isArchiving ? 'Biztosan archiválni szeretnéd ezt a projektet?' : 'Biztosan újra aktiválod ezt a projektet?';

  if (confirm(confirmMsg)) {
    try {
      // Típusbiztos értékadás
      const newStatus: 'archived' | 'ongoing' = isArchiving ? 'archived' : 'ongoing';
      const updatedProject: Partial<Project> = { ...activeProject.value, status: newStatus };

      await projectService.updateProject(activeProject.value.id, updatedProject);

      const index = projectsList.value.findIndex(p => p.id === activeProject.value!.id);
      if (index !== -1 && projectsList.value[index]) {
        projectsList.value[index].status = newStatus;
      }
      activeFilter.value = newStatus;
    } catch (error) {
      console.error("Hiba a projekt állapotának módosításakor", error);
    }
  }
};

const projektTorlese = async () => {
  if (!activeProject.value) return;
  if (confirm('VIGYÁZAT! Biztosan véglegesen törölni szeretnéd ezt a projektet? Ezt nem lehet visszavonni!')) {
    try {
      await projectService.deleteProject(activeProject.value.id);
      projectsList.value = projectsList.value.filter(p => p.id !== activeProject.value!.id);
      selectedProjectId.value = null;
    } catch (error) {
      console.error("Hiba a projekt törlésekor", error);
    }
  }
};

const felhasznaloHozzaadasa = async (user: any) => {
  if (!activeProject.value) return;
  const marBenneVan = activeProject.value.members.some(m => String(m.userId) === String(user.id));

  if (!marBenneVan) {
    try {
      await projectService.inviteMember(activeProject.value.id, String(user.id));

      const ujTag: ProjectMember = {
        userId: String(user.id),
        name: user.nev,
        role: 'Meghívott',
        avatar: user.avatar
      };
      activeProject.value.members.push(ujTag);

    } catch (error) {
      console.error("Hiba a tag hozzáadásakor", error);
    }
  }
};

const tagEltavolitasa = async (userId: string) => {
  if (!activeProject.value) return;
  const eredetiTagok = [...activeProject.value.members];
  activeProject.value.members = activeProject.value.members.filter(m => String(m.userId) !== String(userId));
  activeProject.value.tasks.forEach(task => {
    task.assignedTo = task.assignedTo.filter((id: string) => String(id) !== String(userId));
  });

  try {
    // Használjuk a Partial<Project> formát
    const projectToSave: Partial<Project> = { ...activeProject.value };
    await projectService.updateProject(activeProject.value.id, projectToSave);
  } catch (error) {
    console.error("Hiba a tag eltávolításakor", error);
    activeProject.value.members = eredetiTagok;
  }
};

const megnyitProfil = (userId: string) => {
  window.open(`/user/${userId}`, '_blank');
};

const mutasdTagReszleteit = (member: ProjectMember) => {
  megnyitProfil(member.userId);
};

const ujFeladatKezdese = () => {
  kivalasztottFeladatSzerkesztesre.value = null;
  showTaskCreateModal.value = true;
};

const feladatSzerkesztese = (task: Task) => {
  kivalasztottFeladatSzerkesztesre.value = { ...task };
  showTaskCreateModal.value = true;
};

const feladatMentese = async (taskData: any) => {
  if (!activeProject.value) return;

  try {
    const savedTask = await projectService.saveTask(activeProject.value.id, taskData);

    if (taskData.id) {
      const index = activeProject.value.tasks.findIndex(t => t.id === savedTask.id);
      if (index !== -1) {
        activeProject.value.tasks.splice(index, 1, savedTask);
      }
    } else {
      activeProject.value.tasks.push(savedTask);
    }
  } catch (error) {
    console.error("Hiba a feladat mentésekor", error);
  }
};

const feladatTorlese = async (taskId: number) => {
  if (!activeProject.value) return;

  try {
    await projectService.deleteTask(activeProject.value.id, taskId);
    activeProject.value.tasks = activeProject.value.tasks.filter(t => t.id !== taskId);
  } catch (error) {
    console.error("Hiba a feladat törlésekor", error);
  }
};
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.08); border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: rgba(74, 222, 128, 0.3); }
</style>
